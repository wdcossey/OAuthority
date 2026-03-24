cmake_minimum_required(VERSION 3.20)
project(OAuthority.Native LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Determine .NET Runtime Identifier from CMake generator platform
if(CMAKE_GENERATOR_PLATFORM STREQUAL "ARM64")
  set(RID "win-arm64")
elseif(CMAKE_GENERATOR_PLATFORM STREQUAL "Win32")
  set(RID "win-x86")
else()
  set(RID "win-x64")
endif()

message(STATUS "Building for RID: ${RID}")

add_library(OAuthority.Native SHARED OAuthority.Windows.cpp)

target_link_libraries(OAuthority.Native PRIVATE)

# Place the DLL directly in the runtimes folder expected by the .csproj
set(NATIVE_OUTPUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../runtimes/${RID}/native")

set_target_properties(OAuthority.Native PROPERTIES
  RUNTIME_OUTPUT_DIRECTORY_RELEASE "${NATIVE_OUTPUT_DIR}"
  RUNTIME_OUTPUT_DIRECTORY_DEBUG   "${NATIVE_OUTPUT_DIR}"
  LIBRARY_OUTPUT_DIRECTORY_RELEASE "${NATIVE_OUTPUT_DIR}"
  LIBRARY_OUTPUT_DIRECTORY_DEBUG   "${NATIVE_OUTPUT_DIR}"
  ARCHIVE_OUTPUT_DIRECTORY_RELEASE "${CMAKE_BINARY_DIR}/lib"
  ARCHIVE_OUTPUT_DIRECTORY_DEBUG   "${CMAKE_BINARY_DIR}/lib"
  OUTPUT_NAME "OAuthority.Native"
)
