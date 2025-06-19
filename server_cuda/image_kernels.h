#pragma once
#include <cuda_runtime.h>

extern "C"
__declspec(dllexport)
bool ProcessImage(
    unsigned char* imageData, int width, int height, int channels,
    const char* filterName, double* gpuTimeMs
);
