#include <cstdint>
#include <iostream>
#include <chrono>
#include <random>
#include <vector>

using std::chrono::steady_clock;

__declspec(noinline) int32_t old(int32_t i, int64_t longValue)
{
    if (longValue == 0) return -1;

    return (i << 3) +
        ((longValue & 0x00000000ffffffff) > 0
            ? (longValue & 0x000000000000ffff) > 0
            ? (longValue & 0x00000000000000ff) > 0 ? 0 : 1
            : (longValue & 0x0000000000ff0000) > 0 ? 2 : 3
            : (longValue & 0x0000ffff00000000) > 0
            ? (longValue & 0x000000ff00000000) > 0 ? 4 : 5
            : (longValue & 0x00ff000000000000) > 0 ? 6 : 7);
}

__declspec(noinline) int32_t ternary(int32_t i, int64_t longValue)
{
    if (longValue == 0) return -1;

    int32_t result = i << 3;

    int64_t tmp1 = longValue & 0x00000000ffffffff;
    result += tmp1 == 0 ? 4 : 0;
    longValue = tmp1 == 0 ? longValue : tmp1;

    int64_t tmp2 = longValue & 0x0000ffff0000ffff;
    result += tmp2 == 0 ? 2 : 0;
    longValue = tmp2 == 0 ? longValue : tmp2;

    int64_t tmp3 = longValue & 0x00ff00ff00ff00ff;
    result += tmp3 == 0 ? 1 : 0;

    return result;
}

__declspec(noinline) int32_t ifs(int32_t i, int64_t longValue)
{
    if (longValue == 0) return -1;

    int32_t result = i << 3;

    int64_t tmp1 = longValue & 0x00000000ffffffff;

    if (tmp1 == 0)
        result += 4;
    else
        longValue = tmp1;

    int64_t tmp2 = longValue & 0x0000ffff0000ffff;

    if (tmp2 == 0)
        result += 2;
    else
        longValue = tmp2;

    int64_t tmp3 = longValue & 0x00ff00ff00ff00ff;

    if (tmp3 == 0)
        result += 1;

    return result;
}

#define f ifs

const int count = 500;
const int iterations = 10 * 1000 * 1000;

const std::vector<uint64_t> inputs{
    0xffffffffffffffff, 0xffffffffffffff00, 0xffffffffffff0000, 0xffffffffff000000, 0xffffffff00000000, 0xffffff0000000000, 0xffff000000000000, 0xff00000000000000, 0x0000000000000000,
    0x00000000000000ff, 0x000000000000ff00, 0x0000000000ff0000, 0x00000000ff000000, 0x000000ff00000000, 0x0000ff0000000000, 0x00ff000000000000
};

int main(int argc, char** argv)
{
    for (int i = 0; i < count; i++)
    {
        std::default_random_engine generator(42);
        std::uniform_int_distribution<int> distribution(0, inputs.size() - 1);

        auto start = steady_clock::now();

        int32_t result = 0;

        for (int j = 0; j < iterations; j++)
        {
            int index = distribution(generator);

            result += f(0, inputs[index]);
        }

        volatile int32_t tmp = result;

        auto end = steady_clock::now();

        std::cout << (end - start).count() / (double)iterations << '\n';
    }

    return 0;
}