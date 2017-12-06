#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
/* ----------------------------------------------------------------------------
 * This file was automatically generated by SWIG (http://www.swig.org).
 * Version 2.0.11
 *
 * Do not make changes to this file unless you know what you are doing--modify
 * the SWIG interface file instead.
 * ----------------------------------------------------------------------------- */


using System;
using System.Runtime.InteropServices;

public class AkAuxSendValue : IDisposable {
  private IntPtr swigCPtr;
  protected bool swigCMemOwn;

  internal AkAuxSendValue(IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = cPtr;
  }

  internal static IntPtr getCPtr(AkAuxSendValue obj) {
    return (obj == null) ? IntPtr.Zero : obj.swigCPtr;
  }

  ~AkAuxSendValue() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr != IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AkSoundEnginePINVOKE.CSharp_delete_AkAuxSendValue(swigCPtr);
        }
        swigCPtr = IntPtr.Zero;
      }
      GC.SuppressFinalize(this);
    }
  }

  public ulong listenerID { set { AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_listenerID_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_listenerID_get(swigCPtr); } 
  }

  public uint auxBusID {
    set {
      AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_set(swigCPtr, value);
    } 
    get {
      uint ret = AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_auxBusID_get(swigCPtr);
      return ret;
    } 
  }

  public float fControlValue {
    set {
      AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_set(swigCPtr, value);
    } 
    get {
      float ret = AkSoundEnginePINVOKE.CSharp_AkAuxSendValue_fControlValue_get(swigCPtr);
      return ret;
    } 
  }

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.