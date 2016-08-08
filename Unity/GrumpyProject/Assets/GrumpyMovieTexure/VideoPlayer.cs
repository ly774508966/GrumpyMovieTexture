﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class VideoPlayer : MonoBehaviour
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool DataCallback(IntPtr data, int bytesMax, out int bytesRead);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr CreateTextureCallback(int index, int width, int height);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern IntPtr VPCreate();

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern void VPDestroy(IntPtr player);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern void VPPlay(IntPtr player);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern bool VPIsPlaying(IntPtr player);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern void VPStop(IntPtr player);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern bool VPIsStopped(IntPtr player);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern bool VPOpen(IntPtr player, DataCallback dataCallback, CreateTextureCallback textureCallback);

#if UNITY_IPHONE && !UNITY_EDITOR
    [DllImport ("__Internal")]
#else
    [DllImport("GrumpyMovieTexture")]
#endif
    private static extern void VPUpdate(IntPtr player, float timeStep);

    public string resourceName;

    IntPtr player;
    byte[] asset;
    int assetOffset;
    Texture2D[] textures = new Texture2D[3];
    DataCallback dataCallback;
    CreateTextureCallback textureCallback;

    void OnEnable()
    {
        player = VPCreate();

        asset = Resources.Load<TextAsset>(resourceName).bytes;
        assetOffset = 0;

        dataCallback = (IntPtr data, int bytesMax, out int bytesRead) =>
        {
            int bytesToRead = Math.Min(asset.Length - assetOffset, bytesMax);
            Marshal.Copy(asset, assetOffset, data, bytesToRead);
            assetOffset += bytesToRead;
            bytesRead = bytesToRead;
            return assetOffset < asset.Length;
        };
        textureCallback = (index, width, height) =>
        {
            textures[index] = new Texture2D(width, height, TextureFormat.Alpha8, false, true);
            return textures[index].GetNativeTexturePtr();
        };

        VPOpen(player, dataCallback, textureCallback);
    }

    void OnDisable()
    {
        VPDestroy(player);
        player = IntPtr.Zero;
    }

    void Update()
    {
        VPUpdate(player, Time.deltaTime);
        var renderer = GetComponent<Renderer>();
        renderer.sharedMaterial.SetTexture("_MainYTex", textures[0]);
        renderer.sharedMaterial.SetTextureScale("_MainYTex", new Vector2(1, -1));
        renderer.sharedMaterial.SetTexture("_MainCbTex", textures[1]);
        renderer.sharedMaterial.SetTextureScale("_MainCbTex", new Vector2(1, -1));
        renderer.sharedMaterial.SetTexture("_MainCrTex", textures[2]);
        renderer.sharedMaterial.SetTextureScale("_MainCrTex", new Vector2(1, -1));
    }

    void OnGUI()
    {
        bool isPlaying = VPIsPlaying(player);
        if (GUILayout.Button(isPlaying ? "Stop" : "Play"))
        {
            if (isPlaying)
            {
                VPStop(player);
            }
            else
            {
                if (VPIsStopped(player))
                {
                    assetOffset = 0;
                    VPOpen(player, dataCallback, textureCallback);
                }
                VPPlay(player);
            }
        }
    }
}