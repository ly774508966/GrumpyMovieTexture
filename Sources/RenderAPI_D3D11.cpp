#include "RenderAPI.h"
#include "PlatformBase.h"

// Direct3D 11 implementation of RenderAPI.

#if SUPPORT_D3D11

#include <assert.h>
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"

class RenderAPI_D3D11 : public RenderAPI
{
public:
    RenderAPI_D3D11();
    virtual ~RenderAPI_D3D11() { }

    virtual void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
    {
        switch (type)
        {
        case kUnityGfxDeviceEventInitialize:
        {
            IUnityGraphicsD3D11* d3d = interfaces->Get<IUnityGraphicsD3D11>();
            m_Device = d3d->GetDevice();
            break;
        }
        }
    }

    virtual void UpdateTexture(void* handle, int width, int height, int rowPitch, uint8_t* data)
    {
        ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)handle;
        if (d3dtex == nullptr)
        {
            return;
        }

        ID3D11DeviceContext* ctx = nullptr;
        m_Device->GetImmediateContext(&ctx);
        ctx->UpdateSubresource(d3dtex, 0, nullptr, data, rowPitch, 0);
        ctx->Release();
    }

    virtual void* BeginModifyTexture(void* textureHandle, int textureWidth, int textureHeight, int* outRowPitch);
    virtual void EndModifyTexture(void* textureHandle, int textureWidth, int textureHeight, int rowPitch, void* dataPtr);

private:
    ID3D11Device* m_Device;
};


RenderAPI* CreateRenderAPI_D3D11()
{
    return new RenderAPI_D3D11();
}


RenderAPI_D3D11::RenderAPI_D3D11()
    : m_Device(NULL)
{
}


void* RenderAPI_D3D11::BeginModifyTexture(void* textureHandle, int textureWidth, int textureHeight, int* outRowPitch)
{
    const int rowPitch = textureWidth * 4;
    // Just allocate a system memory buffer here for simplicity
    unsigned char* data = new unsigned char[rowPitch * textureHeight];
    *outRowPitch = rowPitch;
    return data;
}


void RenderAPI_D3D11::EndModifyTexture(void* textureHandle, int textureWidth, int textureHeight, int rowPitch, void* dataPtr)
{
    ID3D11Texture2D* d3dtex = (ID3D11Texture2D*)textureHandle;
    assert(d3dtex);

    ID3D11DeviceContext* ctx = NULL;
    m_Device->GetImmediateContext(&ctx);
    // Update texture data, and free the memory buffer
    ctx->UpdateSubresource(d3dtex, 0, NULL, dataPtr, rowPitch, 0);
    delete[] (unsigned char*)dataPtr;
    ctx->Release();
}


#endif // #if SUPPORT_D3D11
