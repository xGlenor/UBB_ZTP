cmake_minimum_required(VERSION 3.10)
project(ImageProcCuda LANGUAGES CXX CUDA)

set(CMAKE_CXX_STANDARD 14)

# <-- Dodajemy ścieżkę, którą NVCC będzie przeszukiwać pod kątem nagłówków:
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

add_library(ImageProcCuda SHARED
    image_kernels.cu
)

# Dodatkowo podajemy per-target, ale include_directories wystarczy:
# target_include_directories(ImageProcCuda PRIVATE ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(ImageProcCuda PROPERTIES
    CUDA_SEPARABLE_COMPILATION ON
    POSITION_INDEPENDENT_CODE ON
    WINDOWS_EXPORT_ALL_SYMBOLS ON
)
