using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Names {

	private static System.Random random = new System.Random();
	public static string[] names = new string[]{

		"Jeff",
		"Susan",
		"Alice",
		"Greg",
		"Matt",
		"Quentin",
		"Sam",
		"Ben",
		"Gabriele"
	};

	public static string randomName(){
		return names[random.Next(0, names.Length)];
	}
}
