cmake_minimum_required(VERSION 3.18)
project(probability)

set(CMAKE_CXX_STANDARD 20)

add_executable(probability main.cpp)
target_link_libraries(probability yaucl_hashing)