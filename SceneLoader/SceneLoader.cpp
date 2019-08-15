// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "pch.h"
#include "SceneLoader.h"

#include "UtilForIntermingledNamespaces.h"
#include "Bounds3D.h"
#include "GLTFVisitor.h"

using namespace std;
using namespace Microsoft::glTF;
using namespace SceneLoader;

namespace winrt {
    using namespace Windows::ApplicationModel::Core;
    using namespace Windows::Foundation::Collections;
    using namespace Windows::UI::Core;
    using namespace Windows::UI::Composition;
    using namespace Windows::UI::Composition::Scenes;
    using namespace Windows::Storage;
    using namespace Windows::Storage::Streams;
    using namespace Windows::Storage::Pickers;
    using namespace Windows::Foundation::Numerics;
    using namespace Windows::Foundation;
}
using namespace winrt;

namespace winrt::SceneLoaderComponent::implementation
{
    struct MemBuf : std::streambuf
    {
        MemBuf(char* begin, char* end) {
            this->setg(begin, begin, end);
        }
    };

    struct StreamReader : public IStreamReader
    {
        MemBuf m_membuf;

        StreamReader(BYTE* data, UINT32 capacity) :
            m_membuf((char*)data, (char*)(data + capacity))
        {
        }

        virtual ~StreamReader()
        {

        }

        shared_ptr<istream> GetInputStream(const std::string&) const override
        {
            auto spIfStream = make_shared<std::istream>(const_cast<MemBuf*>(&m_membuf));

            if (spIfStream->fail())
            {
                throw exception("failed to open file");
            }
            return spIfStream;
        }
    };

    SceneNode SceneLoader::Load(IBuffer buffer, Compositor compositor)
    {
        auto memoryBuffer = winrt::Windows::Storage::Streams::Buffer::CreateMemoryBufferOverIBuffer(buffer);
        auto memoryBufferReference = memoryBuffer.CreateReference();
        auto data = GetDataPointerFromMemoryBuffer(memoryBufferReference);

        SceneNode worldNode = SceneNode::Create(compositor);
        SceneNode rootNode = SceneNode::Create(compositor);
        worldNode.Children().Append(rootNode);

        //
        // Parses the GLTF file and creates the WUC Scenes objects
        //
        ParseGLTF(data.first, data.second, compositor, rootNode);

        Bounds3D bounds = ComputeTreeBounds(
            rootNode,
            float4x4::identity());

        float lengthX = bounds.Max().x - bounds.Min().x;
        float lengthY = bounds.Max().y - bounds.Min().y;
        float lengthZ = bounds.Max().z - bounds.Min().z;

        float maxDimension = max(lengthX, max(lengthY, lengthZ));

        if (maxDimension > 0.0f)
        {
            float scaleFactor = 300.0f / maxDimension;

            worldNode.Transform().Scale({ scaleFactor, scaleFactor, scaleFactor });
            worldNode.Transform().Translation({ 0.0f, -(bounds.Min().y + bounds.Max().y) * scaleFactor / 2, 0.0f });

        }

        return worldNode;
    }

    void SceneLoader::ParseGLTF(BYTE* data, UINT32 capacity, Compositor& compositor, SceneNode& rootNode)
    {
        auto streamReader = make_shared<StreamReader>(data, capacity);
        auto spifstream = streamReader->GetInputStream("");
        auto resourceReader = make_shared<GLTFResourceReader>(streamReader);

        //////////////////////////////////////////////////////////////////////////////
        //
        // Document
        //
        //////////////////////////////////////////////////////////////////////////////
        Document gltfDoc = Deserialize(*spifstream);
        Validation::Validate(gltfDoc);

        DoIt(gltfDoc, resourceReader, compositor, rootNode);
    }

    void SceneLoader::DoIt(Document& gltfDoc, shared_ptr<GLTFResourceReader> resourceReader, Compositor& compositor, SceneNode& rootNode)
    {
        //////////////////////////////////////////////////////////////////////////////
        //
        // Scene
        //
        //////////////////////////////////////////////////////////////////////////////
        auto scene = gltfDoc.GetDefaultScene();

        shared_ptr<SceneResourceSet> resourceSet = make_shared<SceneResourceSet>(compositor);

        Visit(gltfDoc, DefaultSceneIndex, GLTFVisitor(
            compositor,
            rootNode,
            resourceSet,
            resourceReader,
            gltfDoc,
            scene));

        resourceSet->CreateSceneMaterialObjects();
    }
}
