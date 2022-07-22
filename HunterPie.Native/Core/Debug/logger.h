#pragma once
#if _DEBUG
#define LOG(...) printf_s(__VA_ARGS__); \
                 printf_s("\n");
#else
#define LOG(...)
#endif