﻿#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

using SimpleLocalization.Settings;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;
using System;

namespace SimpleLocalization.Helpers
{
    public static class LocalizatorWebLoader
    {
        public static void DownloadTranslationFile(Action onLoadedEnded)
        {
            switch (LocalizatorSettingsWrapper.DownloadingType)
            {
                case DownloadingType.ManualInEditor:
                    {
                        onLoadedEnded?.Invoke();
                    }
                    break;
                case DownloadingType.AutoInEditor:
                    {
#if !UNITY_EDITOR
                        onLoadedEnded?.Invoke();
#else
                        EditorCoroutineUtility.StartCoroutineOwnerless(StartLoadingFile(LocalizatorSettingsWrapper.ActualTableLink, onLoadedEnded));
#endif
                    }
                    break;
                case DownloadingType.AutoOnDevice:
                    {
#if UNITY_EDITOR
                        onLoadedEnded?.Invoke();
#else
                        LoadOnDevice(onLoadedEnded);
#endif
                    }
                    break;
                case DownloadingType.Always:
                    {
#if UNITY_EDITOR
                        EditorCoroutineUtility.StartCoroutineOwnerless(StartLoadingFile(LocalizatorSettingsWrapper.ActualTableLink, onLoadedEnded));
#else
                        LoadOnDevice(onLoadedEnded);
#endif
                    }
                    break;
            }
        }

        public static void ForceDownloadTranslationFile(Action onLoadedEnded)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(StartLoadingFile(LocalizatorSettingsWrapper.ActualTableLink, onLoadedEnded));
#endif
        }

        private static IEnumerator StartLoadingFile(string link, Action onLoadedEnded)
        {
            UnityWebRequest request = UnityWebRequest.Get(link);

            request.timeout = LocalizatorSettingsWrapper.DownloadingTimeout;
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogWarning("<color=yellow>SIMPLE-LOCALIZATOR ERROR</color>: Network or http error on downloading localizator file.");
                onLoadedEnded?.Invoke();
            }
            else
            {
                LocalizatorLocalFiles.WriteLocalizationFile(request.downloadHandler.text, onLoadedEnded);
            }

            request.Dispose();
        }

        private static void LoadOnDevice(Action onLoadedEnded)
        {
            GameObject gameObject = new GameObject("LocalizatorWebLoader");
            LocalizatorWebLoaderController localizatorWebLoaderController = gameObject.AddComponent<LocalizatorWebLoaderController>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

            onLoadedEnded += () => UnityEngine.Object.Destroy(gameObject);
            localizatorWebLoaderController.StartCoroutine(StartLoadingFile(LocalizatorSettingsWrapper.ActualTableLink, onLoadedEnded));
        }
    }

    public class LocalizatorWebLoaderController : MonoBehaviour
    {
    }
}