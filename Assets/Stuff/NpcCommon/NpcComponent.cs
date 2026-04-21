using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcComponent : MonoBehaviour
{
	public Npc npc;

	protected virtual void Awake()
	{
		SetNpc(GetComponentInParent<Npc>());
	}

	public virtual void SetNpc(Npc npc)
	{
		this.npc = npc;
	}
}
