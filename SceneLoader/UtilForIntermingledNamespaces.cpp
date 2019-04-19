#include "pch.h"

#include "UtilForIntermingledNamespaces.h"

// WinRT
#include <MemoryBuffer.h>

// This class doesn't use any "using namespace XXX" because it mixes C++/WinRT
// objects with WinRT ABI Objects (::Windows::Foundation::IMemoryBufferByteAccess).
// The reason for that is that C++/WinRT doesn't expose ::Windows::Foundation::IMemoryBufferByteAccess
// that is being declared into MemoryBuffer.h.
// https://docs.microsoft.com/en-us/windows/uwp/cpp-and-winrt-apis/interop-winrt-abi

namespace SceneLoader
{
    winrt::Windows::Foundation::MemoryBuffer
        CopyArrayOfBytesToMemoryBuffer(BYTE* data, size_t byteLength)
    {
        winrt::Windows::Foundation::MemoryBuffer mb{ winrt::Windows::Foundation::MemoryBuffer(static_cast<UINT32>(byteLength)) }; // FIXME: Check bounds sizeof(size_t) > sizeof(UINT32) on x64
        winrt::Windows::Foundation::IMemoryBufferReference mbr = mb.CreateReference();
        winrt::com_ptr<Windows::Foundation::IMemoryBufferByteAccess> const mba{ mbr.as<Windows::Foundation::IMemoryBufferByteAccess>() };

        {
            BYTE* bytes = nullptr;
            UINT32 capacity;
            mba->GetBuffer(&bytes, &capacity);
            for (UINT32 i = 0; i < capacity; ++i)
            {
                bytes[i] = data[i];
            }
        }

        return mb;
    }

    // std::span is only available in C++20 :(
    std::pair<BYTE*, UINT32>
        GetDataPointerFromMemoryBuffer(winrt::Windows::Foundation::IMemoryBufferReference memoryBufferReference)
    {
        auto memoryBufferByteAccess = memoryBufferReference.as<Windows::Foundation::IMemoryBufferByteAccess>();

        BYTE* data;
        UINT32 capacity;

        memoryBufferByteAccess->GetBuffer(&data, &capacity);

        return std::make_pair(data, capacity);
    }

    winrt::hstring
        GetHSTRINGFromStdString(const std::string& s)
    {
        size_t keyLenght = s.length() + 1;
        auto wcstring = std::make_unique<wchar_t[]>(keyLenght);
        size_t convertedChars = 0;
        mbstowcs_s(&convertedChars, wcstring.get(), keyLenght, s.c_str(), _TRUNCATE);

        return wcstring.get();
    }

} // namespace SceneLoader
