// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

namespace SceneLoader {

    class Bounds3D
    {
    public:
        Bounds3D(winrt::Windows::UI::Composition::Scenes::SceneBoundingBox sceneBounds);

        Bounds3D(winrt::Windows::Foundation::Numerics::float3 _min = winrt::Windows::Foundation::Numerics::float3(FLT_MAX, FLT_MAX, FLT_MAX),
                 winrt::Windows::Foundation::Numerics::float3 _max = winrt::Windows::Foundation::Numerics::float3(FLT_MIN, FLT_MIN, FLT_MIN));

        static Bounds3D Union(const Bounds3D &lBounds, const Bounds3D &rBounds);
        static Bounds3D Transform(const Bounds3D &srcBounds, const winrt::Windows::Foundation::Numerics::float4x4 &transform);

        winrt::Windows::Foundation::Numerics::float3 Min() const;

        winrt::Windows::Foundation::Numerics::float3 Max() const;        

    private:
        winrt::Windows::Foundation::Numerics::float3 m_min;
        winrt::Windows::Foundation::Numerics::float3 m_max;
    };

    Bounds3D ComputeTreeBounds(winrt::Windows::UI::Composition::Scenes::SceneNode root,
                               winrt::Windows::Foundation::Numerics::float4x4 parentTransform);

    void DecomposeMatrix(
        const std::array<float, 16> matrix,
        winrt::Windows::Foundation::Numerics::float3* pOutScale,
        winrt::Windows::Foundation::Numerics::quaternion* pOutRotation,
        winrt::Windows::Foundation::Numerics::float3* pOutTranslation
        );
}