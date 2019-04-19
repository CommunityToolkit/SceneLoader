#include "pch.h"

#include "Bounds3D.h"

using namespace std;

namespace winrt {
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::UI::Composition::Scenes;
}
using namespace winrt;

namespace SceneLoader
{
    Bounds3D::Bounds3D(SceneBoundingBox sceneBounds)
    {
        m_min = sceneBounds.Min();
        m_max = sceneBounds.Max();
    }

    Bounds3D::Bounds3D(float3 _min, float3 _max)
    {
        m_min = _min;
        m_max = _max;
    }

    Bounds3D
    Bounds3D::Union(const Bounds3D &lBounds, const Bounds3D &rBounds)
    {
        winrt::Windows::Foundation::Numerics::float3 newMin;
        winrt::Windows::Foundation::Numerics::float3 newMax;

        newMin.x = min(lBounds.Min().x, rBounds.Min().x);
        newMin.y = min(lBounds.Min().y, rBounds.Min().y);
        newMin.z = min(lBounds.Min().z, rBounds.Min().z);

        newMax.x = max(lBounds.Max().x, rBounds.Max().x);
        newMax.y = max(lBounds.Max().y, rBounds.Max().y);
        newMax.z = max(lBounds.Max().z, rBounds.Max().z);

        return Bounds3D(newMin, newMax);
    }

    Bounds3D
    Bounds3D::Transform(const Bounds3D &srcBounds, const winrt::Windows::Foundation::Numerics::float4x4 &srcToDestTransform)
    {
        float3 newMin(FLT_MAX, FLT_MAX, FLT_MAX);
        float3 newMax(-FLT_MAX, -FLT_MAX, -FLT_MAX);

        {
            float3 boxVertices[8];

            // Setup a cube, first 4 verties will be min (z) plane, 2nd 4 vertices will be max (z) plane
            // First vertex in each plane will be at min (x,y) winding clockwise around to rest at (xMax, yMin)

            boxVertices[0] = srcBounds.Min();
            boxVertices[6] = srcBounds.Max();

            boxVertices[1] = boxVertices[0];
            boxVertices[1].y = boxVertices[6].y;

            boxVertices[2] = boxVertices[1];
            boxVertices[2].x = boxVertices[6].x;

            boxVertices[3] = boxVertices[2];
            boxVertices[3].y = boxVertices[0].y;

            boxVertices[4] = boxVertices[0];
            boxVertices[4].z = boxVertices[6].z;

            boxVertices[5] = boxVertices[4];
            boxVertices[5].y = boxVertices[6].y;

            boxVertices[7] = boxVertices[6];
            boxVertices[7].y = boxVertices[0].y;

            for (int i = 0; i < 8; i++)
            {
                boxVertices[i] = transform(boxVertices[i], srcToDestTransform);

                newMin.x = min(newMin.x, boxVertices[i].x);
                newMin.y = min(newMin.y, boxVertices[i].y);
                newMin.z = min(newMin.z, boxVertices[i].z);

                newMax.x = max(newMax.x, boxVertices[i].x);
                newMax.y = max(newMax.y, boxVertices[i].y);
                newMax.z = max(newMax.z, boxVertices[i].z);
            }
        }

        return Bounds3D(newMin, newMax);
    }


    float3 Bounds3D::Min() const
    {
        return m_min;
    }

    float3 Bounds3D::Max() const
    {
        return m_max;
    }


    Bounds3D ComputeTreeBounds(winrt::Windows::UI::Composition::Scenes::SceneNode root);

    void DecomposeMatrix(
        const std::array<float, 16> matrix,
        winrt::Windows::Foundation::Numerics::float3* pOutScale,
        winrt::Windows::Foundation::Numerics::quaternion* pOutRotation,
        winrt::Windows::Foundation::Numerics::float3* pOutTranslation
    );

    Bounds3D ComputeTreeBounds(
        SceneNode root,
        float4x4 parentToWorldTransform
        )
    {
        Bounds3D retBounds;

        float4x4 localToWorldTransform = parentToWorldTransform;

        localToWorldTransform *= make_float4x4_scale(root.Transform().Scale());
        localToWorldTransform *= make_float4x4_from_quaternion(root.Transform().Orientation());
        localToWorldTransform *= make_float4x4_translation(root.Transform().Translation());

        // Check if we have a mesh attached
        auto firstComponent = root.Components().First();

        if (firstComponent && firstComponent.HasCurrent())
        {
            auto meshRenderer = firstComponent.Current().as<SceneMeshRendererComponent>();

            if (meshRenderer)
            {
                auto mesh = meshRenderer.Mesh();

                if (mesh)
                {
                    Bounds3D localBounds = mesh.Bounds();

                    retBounds = Bounds3D::Transform(localBounds, localToWorldTransform);
                }
            }
        }

        for (UINT i = 0; i < root.Children().Size(); i++)
        {
            retBounds = Bounds3D::Union(retBounds, ComputeTreeBounds(root.Children().GetAt(i), localToWorldTransform));
        }

        return retBounds;
    }

    void DecomposeMatrix(
        const std::array<float, 16> matrix,
        float3* pOutScale,
        quaternion* pOutRotation,
        float3* pOutTranslation)
    {
        float4x4 inputMatrix(
            matrix[0], matrix[1], matrix[2], matrix[3],
            matrix[4], matrix[5], matrix[6], matrix[7],
            matrix[8], matrix[9], matrix[10], matrix[11],
            matrix[12], matrix[13], matrix[14], matrix[15]
        );

        bool fDecomposeResult = decompose(
            inputMatrix,
            pOutScale,
            pOutRotation,
            pOutTranslation);

        assert(fDecomposeResult);
    }
} // namespace SceneLoader