using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class FindObjectsWithTags
{

	public static GameObject[] FindGameObjectsWithTags(params string[] tags)
	{
		var all = new List<GameObject>() ;

		foreach(string tag in tags)
		{
			var temp = GameObject.FindGameObjectsWithTag(tag).ToList() ;
			all = all.Concat(temp).ToList() ;
		}

		return all.ToArray() ;
	}
}
