

using UnityEngine;
using UnityEditor;

#if !UNITY_PACKING
public class AudioImportAutoSetting : AssetPostprocessor
{
	// 导入音频时
	private void OnPostprocessAudio(AudioClip clip)
	{
		var audio = (AudioImporter)assetImporter;
		AudioImporterSampleSettings settings = new AudioImporterSampleSettings();

		// 这里根据音量的时间长度演示 导入音频时自动设置的情况，大家可以根据需要自行更改即可
		// 音频时间长度小于 1 秒
		if (clip.length < 1)
		{
			settings.loadType = AudioClipLoadType.DecompressOnLoad;
		}
        // 音频时间长度大于 1 秒 ，小于 3 秒
        else if (clip.length < 3) 
		{
			settings.loadType = AudioClipLoadType.CompressedInMemory;
		}
		// 音频时间长度大于 3 秒 
		else
		{
			settings.loadType = AudioClipLoadType.Streaming;
		}

		settings.quality = 0.7f;
		settings.compressionFormat = AudioCompressionFormat.Vorbis;

		// 进行设置
		audio.defaultSampleSettings = settings;

		// 预载音频数据为 false 
		audio.preloadAudioData = false;
	}
}

#endif