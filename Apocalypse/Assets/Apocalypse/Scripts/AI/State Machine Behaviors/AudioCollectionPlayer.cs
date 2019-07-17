using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCollectionPlayer : AIStateMachineLink
{
    //Inspectors
    [SerializeField] ComChannels _commandChannel = ComChannels.ComChannel1;
    [SerializeField] AudioCollection _collection = null;
    [SerializeField] CustomCurve _customCurve = null;
    [SerializeField] LayerList _layerList = null;
   

    //private
    int _prevCom = 0;
    int _comChannelHash = 0;
    AudioManager _manager = null;


    /*********************************************************/
    // Called by Unity before first frame
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        _manager = AudioManager.instance;
        _prevCom = 0;
        if (_comChannelHash == 0)
        {
            
            _comChannelHash = Animator.StringToHash(_commandChannel.ToString());
            Debug.Log(_commandChannel.ToString());
        }
    }


    /*********************************************************/
    // Called by Unity each frame
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        //no sounds if layer weight is 0
        if (layerIndex != 0 && animator.GetLayerWeight(layerIndex).Equals(0.0f)) return;
        if (!_stateMachine) return;


        if (_layerList)
        {
            for(int i = 0; i < _layerList.count; i++)
            {
                if (_stateMachine.IsLayerActive(_layerList[i])) return;
            }
        }
        //if custom curve exists use that as animator curve
        int customCommand = (_customCurve == null) ? 0 : Mathf.FloorToInt(_customCurve.Evaluate(animatorStateInfo.normalizedTime - (long)animatorStateInfo.normalizedTime));
        int com;
        
        if (customCommand != 0) com = customCommand;
        else com = Mathf.FloorToInt(animator.GetFloat(_comChannelHash));
        //com = Mathf.FloorToInt(animator.GetFloat(_comChannelHash));
        //check if this command is differnt from prev command
        if (_prevCom != com && com > 0 && _manager && _collection && _stateMachine)
        {
            
            //the bank we wish to use
            int bank = Mathf.Max(0, Mathf.Min(com - 1, _collection.bankCount - 1));
            _manager.PlayOneShotSound(_collection.audioGroup, _collection[bank], _stateMachine.transform.position,_collection.volume, _collection.spatialBlend, _collection.priority);
        }
        
        _prevCom = com;
    }
}
