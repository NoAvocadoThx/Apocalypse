using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioLayer
{
    public AudioClip clip = null;
    public AudioCollection collection = null;
    public int bank = 0;
    public bool isLooping = true;
    public float time = 0.0f;
    public float duration = 0.0f;
    public bool isMuted = false;
}
//define a interface
public interface ILayeredAudioSource
{
    bool Play(AudioCollection pool, int bank, int layer, bool looping = true);
    void Stop(int layerIndex);
    void Mute(int layerIndex, bool mute);
    void Mute(bool mute);
}

public class LayeredAudioSource:ILayeredAudioSource
{
    public AudioSource audioSource { get { return _audioSource; } }

    private AudioSource _audioSource = null;
    List<AudioLayer> _audioLayers = new List<AudioLayer>();
    int _activeLayer = -1;


    /*********************************************************/
    //class constructor
    public LayeredAudioSource(AudioSource src,int layers)
    {
        if (src && layers > 0)
        {
            _audioSource = src;
            for(int i = 0; i < layers; i++)
            {
                AudioLayer newLayer = new AudioLayer();
                newLayer.collection = null;
                newLayer.duration = 0.0f;
                newLayer.time = 0.0f;
                newLayer.isLooping = false;
                newLayer.bank = 0;
                newLayer.isMuted = false;
                newLayer.clip = null;
                //add layer to stack
                _audioLayers.Add(newLayer);
            }
        }

    }


    /*********************************************************/
    public bool Play(AudioCollection collection, int bank, int layer, bool isLoop = true)
    {
        //check if in range
        if (layer >= _audioLayers.Count) return false;
        //get the layer we want to configrue
        AudioLayer audioLayer = _audioLayers[layer];
        //if we are looping the collection we want, then return
        if (audioLayer.collection == collection && audioLayer.isLooping == isLoop && bank == audioLayer.bank) return true;
        //else we set
        audioLayer.collection = collection;
        audioLayer.bank = bank;
        audioLayer.isLooping = isLoop;
        audioLayer.duration = 0.0f;
        audioLayer.time = 0.0f;
        audioLayer.isMuted = false;
        audioLayer.clip = null;
        return true;
    }


    /*********************************************************/
    public void Stop(int layerIndex)
    {
        if (layerIndex >= _audioLayers.Count) return;
        AudioLayer layer = _audioLayers[layerIndex];
        if (layer!=null)
        {
            //time to play
            layer.isLooping = false;
            layer.time = layer.duration;
        }
    }


    /*********************************************************/
    //mute
    public void Mute(int layerIndex,bool mute)
    {
        if (layerIndex >= _audioLayers.Count) return;
        AudioLayer layer = _audioLayers[layerIndex];
        if (layer != null)
        {
            layer.isMuted = mute;
        }
    }

    /*********************************************************/
    //mute all layers
    public void Mute(bool mute)
    {
       for(int i = 0; i < _audioLayers.Count; i++)
        {
            Mute(i, mute);
        }
    }

    /*********************************************************/
    //Updates the time of all layered clips and makes sure that the audio source
    //is playing the clip on the highest layer.
    public void Update()
    {
        // Used to record the highest layer with a clip assigned and still playing
        int newActiveLayer = -1;
        bool refreshAudioSource = false;

        // Update the stack each frame by iterating the layers (Working backwards)
        for (int i = _audioLayers.Count - 1; i >= 0; i--)
        {
            // Layer being processed
            AudioLayer layer = _audioLayers[i];

            // Ignore unassigned layers
            if (layer.collection == null) continue;

            // Update the internal playhead of the layer		
            layer.time += Time.deltaTime;

            // If it has exceeded its duration then we need to take action
            if (layer.time > layer.duration)
            {
                // If its a looping sound OR the first time we have set up this layer
                // we need to assign a new clip from the pool assigned to this layer
                if (layer.isLooping || layer.clip == null)
                {

                    // Fetch a new clip from the pool
                    AudioClip clip = layer.collection[layer.bank];

                    // Calculate the play position based on the time of the layer and store duration
                    // aka prev playing clip
                    if (clip == layer.clip)
                        layer.time = layer.time % layer.clip.length;
                    else
                        layer.time = 0.0f;

                    layer.duration = clip.length;
                    layer.clip = clip;

                    // This is a layer that has focus so we need to chose and play
                    // a new clip from the pool
                    // check if this is the highest priority layer
                    if (newActiveLayer < i)
                    {
                        // This is the active layer index
                        newActiveLayer = i;
                        // We need to issue a play command to the audio source
                        refreshAudioSource = true;
                    }
                }
                //is not looping, layer time>play time
                else
                {
                    // The clip assigned to this layer has finished playing and is not set to loop
                    // so clear the later and reset its status ready for reuse in the future
                    layer.clip = null;
                    layer.collection = null;
                    layer.duration = 0.0f;
                    layer.bank = 0;
                    layer.isLooping = false;
                    layer.time = 0.0f;
                }
            }
            // Else this layer is playing
            else
            {
                // If this is the highest layer then record that....its the clip currently playing
                if (newActiveLayer < i) newActiveLayer = i;
            }
        }

        // If we found a new active layer (or none)
        if (newActiveLayer != _activeLayer || refreshAudioSource)
        {
            // Previous layer expired and no new layer so stop audio source - there are no active layers
            if (newActiveLayer == -1)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }
            // We found an active layer but its different than the previous update so its time to switch
            // the audio source to play the clip on the new layer
            else
            {
                // Get the layer
                AudioLayer layer = _audioLayers[newActiveLayer];

                _audioSource.clip = layer.clip;
                _audioSource.volume = layer.isMuted ? 0.0f : layer.collection.volume;
                _audioSource.spatialBlend = layer.collection.spatialBlend;
                _audioSource.time = layer.time;
                _audioSource.loop = false;
                //assign audio collection to correct mixer output
                _audioSource.outputAudioMixerGroup = AudioManager.instance.GetAudioGroupFromTrackName(layer.collection.audioGroup);
                _audioSource.Play();
            }
        }

        // Remember the currently active layer for the next update check
        //check isMuted
        _activeLayer = newActiveLayer;

        if (_activeLayer != -1 && _audioSource)
        {
            AudioLayer audioLayer = _audioLayers[_activeLayer];
            if (audioLayer.isMuted) _audioSource.volume = 0.0f;
            else _audioSource.volume = audioLayer.collection.volume;
        }
    }
}
