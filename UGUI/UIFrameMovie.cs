using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIFrameMovie : MonoBehaviour
{
	public Transform[] elements;

	public void Play()
	{
		Stop();
		Play(0);
	}
	
	public void Play(int index)
	{
		if (elements == null)
			return;

		if (index >= elements.Length)
			index = 0;

		Transform element = elements[index];
		if(element == null)
			return;
		
		element.DOScale(Vector3.one * 1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() => 
		{
			index++;
			Play(index);	
		});
	}

	public void Stop()
	{
		if (elements == null)
			return;

		for (int i = 0; i < elements.Length; i++)
		{
			Transform element = elements[i];
			if(element == null)
				continue;

			element.localScale = Vector3.one;
			element.DOKill();
		}
	}
}
