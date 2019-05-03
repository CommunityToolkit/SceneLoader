// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#pragma once

namespace SceneLoader {
    winrt::Windows::Foundation::MemoryBuffer CopyArrayOfBytesToMemoryBuffer(BYTE* data, size_t byteLength);

    std::pair<BYTE*, UINT32> GetDataPointerFromMemoryBuffer(winrt::Windows::Foundation::IMemoryBufferReference);

    winrt::hstring GetHSTRINGFromStdString(const std::string& s);
}