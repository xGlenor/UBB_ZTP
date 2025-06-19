#include "image_kernels.h"
#include <cstdio>
#include <cstring>

__global__ void grayscaleKernel(unsigned char* img, int w, int h, int c) {
    int x = blockIdx.x * blockDim.x + threadIdx.x;
    int y = blockIdx.y * blockDim.y + threadIdx.y;
    if (x >= w || y >= h) return;
    int idx = (y * w + x) * c;
    unsigned char r = img[idx], g = img[idx+1], b = img[idx+2];
    unsigned char gray = (unsigned char)((r+g+b)/3);
    img[idx] = img[idx+1] = img[idx+2] = gray;
}

__global__ void blurKernel(
    const unsigned char* d_in,
    unsigned char* d_out,
    int w, int h, int c)
{
    int x = blockIdx.x*blockDim.x + threadIdx.x;
    int y = blockIdx.y*blockDim.y + threadIdx.y;
    if (x >= w || y >= h) return;

    int idx = (y*w + x)*c;
    int sum[3] = {0,0,0};
    int count = 0;

    for(int dy=-1; dy<=1; ++dy){
        for(int dx=-1; dx<=1; ++dx){
            int nx = x + dx, ny = y + dy;
            if (nx>=0 && nx<w && ny>=0 && ny<h){
                int nidx = (ny*w + nx)*c;
                sum[0] += d_in[nidx + 0];
                sum[1] += d_in[nidx + 1];
                sum[2] += d_in[nidx + 2];
                ++count;
            }
        }
    }
    // uśredniamy
    d_out[idx + 0] = sum[0] / count;
    d_out[idx + 1] = sum[1] / count;
    d_out[idx + 2] = sum[2] / count;
}

extern "C"
__declspec(dllexport)
bool ProcessImage(
    unsigned char* imageData, int width, int height, int channels,
    const char* filterName, double* gpuTimeMs
) {
    size_t numBytes = width * height * channels * sizeof(unsigned char);
    unsigned char* d_img = nullptr;

    // Alokacja GPU
    if (cudaMalloc(&d_img, numBytes) != cudaSuccess) return false;
    if (cudaMemcpy(d_img, imageData, numBytes, cudaMemcpyHostToDevice) != cudaSuccess) {
        cudaFree(d_img); return false;
    }

    // Ustawienie siatki
    dim3 block(16, 16);
    dim3 grid((width+15)/16, (height+15)/16);

    // Pomiary czasu GPU
    cudaEvent_t start, stop;
    cudaEventCreate(&start);
    cudaEventCreate(&stop);
    cudaEventRecord(start);

    // Wybór kernela
    if (strcmp(filterName, "grayscale") == 0) {
        grayscaleKernel<<<grid, block>>>(d_img, width, height, channels);
    }
    else if (strcmp(filterName, "blur") == 0) {
        blurKernel<<<grid, block>>>(d_img, d_img, width, height, channels);
    }
    else {
        // domyślnie grayscale
        grayscaleKernel<<<grid, block>>>(d_img, width, height, channels);
    }

    cudaEventRecord(stop);
    cudaEventSynchronize(stop);
    float ms = 0;
    cudaEventElapsedTime(&ms, start, stop);
    *gpuTimeMs = ms;

    // Kopiowanie z powrotem
    cudaMemcpy(imageData, d_img, numBytes, cudaMemcpyDeviceToHost);

    // Cleanup
    cudaFree(d_img);
    cudaEventDestroy(start);
    cudaEventDestroy(stop);
    return true;
}
