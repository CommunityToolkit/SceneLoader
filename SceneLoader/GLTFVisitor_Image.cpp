#include "pch.h"

#include "GLTFVisitor.h"
#include "wincodec.h"

using namespace std;
using namespace Microsoft::glTF;

namespace winrt {
    using namespace Windows::Graphics;
    using namespace Windows::Graphics::DirectX;
    using namespace Windows::UI::Composition;
}
using namespace winrt;

namespace SceneLoader
{
    // Image
    void GLTFVisitor::operator()(const Image& image, VisitState alreadyVisited, const VisitDefaultAction&)
    {
        if (alreadyVisited == VisitState::New)
        {
            std::vector<uint8_t> imageData = m_gltfResourceReader->ReadBinaryData(m_gltfDocument, image);

            const void* pSource = static_cast<const void*>(imageData.data());

            // Create input stream for memory
            com_ptr<IWICImagingFactory> cpWIC;
            winrt::check_hresult(CoCreateInstance(
                CLSID_WICImagingFactory,
                NULL,
                CLSCTX_INPROC_SERVER,
                __uuidof(cpWIC),
                (LPVOID*)&cpWIC));

            com_ptr<IWICStream> cpStream;
            winrt::check_hresult(cpWIC->CreateStream(cpStream.put()));
            winrt::check_hresult(cpStream->InitializeFromMemory(static_cast<BYTE*>(const_cast<void*>(pSource)),
                static_cast<UINT>(imageData.size())));

            com_ptr<IWICBitmapDecoder> cpDecoder;
            winrt::check_hresult(cpWIC->CreateDecoderFromStream(cpStream.get(), nullptr, WICDecodeMetadataCacheOnDemand, cpDecoder.put()));

            com_ptr<IWICBitmapFrameDecode> cpSource;
            winrt::check_hresult(cpDecoder->GetFrame(0, cpSource.put()));

            UINT imageWidth = 0;
            UINT imageHeight = 0;
            winrt::check_hresult(cpSource->GetSize(&imageWidth, &imageHeight));
            SizeInt32 size{ static_cast<int32_t>(imageWidth), static_cast<int32_t>(imageHeight) }; // FIXME: conversion from 'UINT' to 'int32_t' requires a narrowing conversion
            DirectXPixelFormat pixelFormat = DirectXPixelFormat::B8G8R8A8UIntNormalized; // Warning: SceneResourceSet::EnsureMipMapSurfaceId hard codes these values
            DirectXAlphaMode alphaMode = DirectXAlphaMode::Premultiplied; // Warning: SceneResourceSet::EnsureMipMapSurfaceId hard codes these values

            CompositionMipmapSurface mipmap = EnsureMipMapSurfaceId(
                image.id,
                size,
                pixelFormat,
                alphaMode
				);

			com_ptr<ID2D1Bitmap> cpCurrentSourceBitmap;

			// Create highest resolution source bitmap
			{
				com_ptr<ID2D1DeviceContext> cpD2DContext;

				// Create Scalar
				com_ptr<IWICBitmapScaler> cpScaler;
				winrt::check_hresult(cpWIC->CreateBitmapScaler(cpScaler.put()));

				winrt::check_hresult(cpScaler->Initialize(
					cpSource.get(),                     // Bitmap source to scale.
					imageWidth,                         // Scale width to half of original.
					imageHeight,                        // Scale height to half of original.
					WICBitmapInterpolationModeFant));   // Use Fant mode interpolation.

				com_ptr<IWICFormatConverter> cpConverter;

				winrt::check_hresult(cpWIC->CreateFormatConverter(cpConverter.put()));
				winrt::check_hresult(cpConverter->Initialize(
					cpScaler.get(),
					/*WicPixelFormatFromDirectXPixelFormat(pixelFormat, alphaMode)*/GUID_WICPixelFormat32bppPBGRA,
					WICBitmapDitherTypeNone,
					nullptr,
					0.0f,
					WICBitmapPaletteTypeMedianCut));

				CompositionDrawingSurface cpDrawingSurface = mipmap.GetDrawingSurfaceForLevel(0);
				com_ptr<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop> cpDrawingSurfaceInterop = cpDrawingSurface.as<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop>();

				POINT surfaceUpdateOffset;
				winrt::check_hresult(cpDrawingSurfaceInterop->BeginDraw(
					nullptr,
					IID_PPV_ARGS(cpD2DContext.put()),
					&surfaceUpdateOffset));

				com_ptr<ID2D1BitmapRenderTarget> cpCompatibleRenderTarget;
				winrt::check_hresult(cpD2DContext->CreateCompatibleRenderTarget(cpCompatibleRenderTarget.put()));

				winrt::check_hresult(cpCompatibleRenderTarget->CreateBitmapFromWicBitmap(
					cpConverter.get(),
					nullptr,
					cpCurrentSourceBitmap.put()));

				winrt::check_hresult(cpDrawingSurfaceInterop->EndDraw());
			}

			float sourceBitmapDpiX, sourceBitmapDpiY;
				
			cpCurrentSourceBitmap->GetDpi(&sourceBitmapDpiX, &sourceBitmapDpiY);

			for (UINT i = 0; i < mipmap.LevelCount(); ++i)
            {
				CompositionDrawingSurface cpDrawingSurface = mipmap.GetDrawingSurfaceForLevel(i);
				com_ptr<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop> cpDrawingSurfaceInterop = cpDrawingSurface.as<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop>();
				com_ptr<ID2D1DeviceContext> cpD2DContext;

#ifndef NDEBUG
				{
					D2D1_SIZE_U sourceSize2 = cpCurrentSourceBitmap->GetPixelSize();

					assert(sourceSize2.width == imageWidth);
					assert(sourceSize2.height == imageHeight);
				}
#endif

				POINT surfaceUpdateOffset;
				winrt::check_hresult(cpDrawingSurfaceInterop->BeginDraw(
					nullptr,
					IID_PPV_ARGS(cpD2DContext.put()),
					&surfaceUpdateOffset));
				
                D2D1_RECT_F destRect;
                destRect.left = (float)surfaceUpdateOffset.x;
                destRect.top = (float)surfaceUpdateOffset.y;
                destRect.right = (float)(destRect.left + imageWidth);
                destRect.bottom = (float)(destRect.top + imageHeight);

                cpD2DContext->SetPrimitiveBlend(D2D1_PRIMITIVE_BLEND_COPY);

                cpD2DContext->DrawBitmap(
                    cpCurrentSourceBitmap.get(),
                    &destRect,
                    1.0f,
                    D2D1_BITMAP_INTERPOLATION_MODE_LINEAR
					);

                // For debugging, turn this on to clobber the contents with red
#if 0
                com_ptr<ID2D1SolidColorBrush> cpSolidColorBrush;
                winrt::check_hresult(cpD2DContext->CreateSolidColorBrush(D2D1::ColorF::ColorF(1.0f, 0.0f, 0.0f), cpSolidColorBrush.put()));

                cpD2DContext->FillRectangle(
                    &destRect,
                    cpSolidColorBrush.get()
                );
#endif

                winrt::check_hresult(cpD2DContext->Flush());

                winrt::check_hresult(cpDrawingSurfaceInterop->EndDraw());

                // Update image size
#undef max
                imageWidth = std::max(imageWidth / 2, 1U);
                imageHeight = std::max(imageHeight / 2, 1U);

				// Now we need to generate the next level source bitmap that's going to be used for 
				// the next imagewidth/height.  Note that imageWidth/Height have already been divided
				// by 2.
				com_ptr<ID2D1BitmapRenderTarget> cpNewD2DTarget;

				D2D1_SIZE_F newDesiredSizeF = D2D1::SizeF(static_cast<float>(imageWidth) / sourceBitmapDpiX, static_cast<float>(imageHeight) / sourceBitmapDpiY);

				// Compatible Target should match format
				winrt::check_hresult(cpD2DContext->CreateCompatibleRenderTarget(
					newDesiredSizeF,
					D2D1::SizeU(imageWidth, imageHeight),
					cpCurrentSourceBitmap->GetPixelFormat(),
					D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS_NONE,
					cpNewD2DTarget.put()
				    ));

				cpNewD2DTarget->BeginDraw();

				cpNewD2DTarget->DrawBitmap(
					cpCurrentSourceBitmap.get(),
					D2D1::RectF(0.0f, 0.0f, newDesiredSizeF.width, newDesiredSizeF.height),
					1.0f,
					D2D1_BITMAP_INTERPOLATION_MODE_LINEAR
					);

				winrt::check_hresult(cpNewD2DTarget->Flush());

				winrt::check_hresult(cpNewD2DTarget->EndDraw());

				com_ptr<ID2D1Bitmap> cpNewBitmap;
				winrt::check_hresult(cpNewD2DTarget->GetBitmap(cpNewBitmap.put()));

				// Now that we've generated the next level of bitmap, replace our current one
				cpCurrentSourceBitmap = cpNewBitmap;
            }
        }
    }
}
