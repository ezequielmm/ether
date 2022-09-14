using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPointerRunable
{
    public PointerOrigin PointerType { get; }
    public void Run(string originId, string targetId);
    public void OnSelect();
    public void OnCancel();
}
