using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class loadAndUnloadBankButton : MonoBehaviour
{
    public AK.Wwise.Bank myBankToLoadOrUnload = new AK.Wwise.Bank();

    public void loadMyBank()
    {
        myBankToLoadOrUnload.Load(false,false);
    }

    public void unloadMyBank()
    {
        myBankToLoadOrUnload.Unload();
    }
}
