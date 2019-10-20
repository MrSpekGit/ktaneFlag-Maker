using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using KModkit;
using System.Text.RegularExpressions;

public class vexillologyScript : MonoBehaviour 
{
	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMSelectable[] HorizontalFlag;
	public KMSelectable[] VerticalFlag;
	public KMSelectable[] SwedishFlag;
	public KMSelectable[] NorwegianFlag;
	public KMSelectable[] ColourButtons;
	public KMSelectable FlagTopSubmit;

	public Renderer[] FlagPartColourHorz;
	public Renderer[] FlagPartColourVert;
	public Renderer[] FlagPartColourSwe;
	public Renderer[] FlagPartColourNor;
	public Renderer FlagTopSubmitRen;

	public Material[] butColour;

	public AudioClip[] anthems;	

	string[] coloursStrings = { "Red", "Orange", "Green", "Yellow", "Blue", "Aqua", "White", "Black"};
	string[] coloursStringsCap = { "RED", "ORANGE", "GREEN", "YELOW", "BLUE", "AQUA", "WHITE", "BLACK"};

	//logging
	static int moduleIdCounter = 1;
	bool _issolved = false;
	int moduleId;
	private bool moduleSolved;
	int ActiveColour = 8;
	int ActiveFlag;
	int ActiveFlagTop1;
	int ActiveFlagTop2;
	int ActiveFlagTop3;
	int FlagColour1 = 10;
	int FlagColour2 = 10;
	int FlagColour3 = 10;
	int AnswerColour1 = 10;
	int AnswerColour2 = 10;
	int AnswerColour3 = 10;
	int SubmitTime = 10;
	int AnthemNumber = 100;
	bool _ChadRomania = false;
	bool colourSerialMatch = false;
	bool PS2Present = false;
	bool TwoorMoreIncPresent = false;
	bool LastDigit5orBigger = false;
	bool HasEmptyPortPlate = false;

	void Awake()
	{
		moduleId = moduleIdCounter++;

		for (int i = 0; i < ColourButtons.Length; i++) {
			int j = i;
			ColourButtons [j].OnInteract += delegate () {
				PressedColour (j);
				return false;
			};
		}

		for (int i = 0; i < HorizontalFlag.Length; i++) {
			int j = i;
			HorizontalFlag [j].OnInteract += delegate () {
				PressedHorzFlag (j);
				return false;
			};
		}

		for (int i = 0; i < VerticalFlag.Length; i++) {
			int j = i;
			VerticalFlag [j].OnInteract += delegate () {
				PressedVertFlag (j);
				return false;
			};
		}

		for (int i = 0; i < SwedishFlag.Length; i++) {
			int j = i;
			SwedishFlag [j].OnInteract += delegate () {
				PressedSweFlag (j);
				return false;
			};
		}

		for (int i = 0; i < NorwegianFlag.Length; i++) {
			int j = i;
			NorwegianFlag [j].OnInteract += delegate () {
				PressedNorFlag (j);
				return false;
			};
		}
		FlagTopSubmit.OnInteract += delegate () { PressedSubmit(); return false;};
	}

	void Start () 
	{
		foreach (object[] plate in Bomb.GetPortPlates())
        {
            if (plate.Length == 0)
            {
                HasEmptyPortPlate = true;
                break;
            }
        }

		Startup();
		//Flag Top Colour Generator 
		ActiveFlagTop1 = UnityEngine.Random.Range(0, 8);
		ActiveFlagTop2 = UnityEngine.Random.Range(0, 8);
		ActiveFlagTop3 = UnityEngine.Random.Range(0, 8);
		Debug.LogFormat ("[Vexillology #{0}] Flag Top Colours are {1}, {2} and {3}", moduleId, coloursStrings[ActiveFlagTop1], coloursStrings[ActiveFlagTop2], coloursStrings[ActiveFlagTop3]);

		//Flag Shape Generator
		int FlagGenerator = UnityEngine.Random.Range(0, 4);
		ActiveFlag = FlagGenerator;
		if (FlagGenerator == 0){
			Debug.LogFormat ("[Vexillology #{0}] Flag Type: Horizontal Flag", moduleId);
			for (int i = 0; i < HorizontalFlag.Length; i++){
				HorizontalFlag[i].gameObject.SetActive (true);
			}
		}
		if (FlagGenerator == 1){
			Debug.LogFormat ("[Vexillology #{0}] Flag Type: Vertical Flag", moduleId);
			for (int i = 0; i < VerticalFlag.Length; i++){
				VerticalFlag[i].gameObject.SetActive (true);
			}
		}
		if (FlagGenerator == 2){
			Debug.LogFormat ("[Vexillology #{0}] Flag Type: 2-Colour Nordic Cross Flag", moduleId);
			for (int i = 0; i < SwedishFlag.Length; i++){
				SwedishFlag[i].gameObject.SetActive (true);
			}
		}
		if (FlagGenerator == 3){
			Debug.LogFormat ("[Vexillology #{0}] Flag Type: 3-Colour Nordic Cross Flag", moduleId);
			for (int i = 0; i < NorwegianFlag.Length; i++){
				NorwegianFlag[i].gameObject.SetActive (true);
			}
		}

		AnswerGenerator1();
		AnswerGenerator2();



		if (ActiveFlag != 2){
			AnswerGenerator3();
			TransposingCheck();
			Debug.LogFormat ("[Vexillology #{0}] The answers are {1}, {2} and {3}", moduleId, coloursStrings[AnswerColour1], coloursStrings[AnswerColour2], coloursStrings[AnswerColour3]);
		} else {
			TransposingCheck();
			Debug.LogFormat ("[Vexillology #{0}] The answers are {1} and {2}", moduleId, coloursStrings[AnswerColour1], coloursStrings[AnswerColour2]);
		}

		CheckCountries();
		StartCoroutine(FlagTopCirulator());
	}

	void Startup(){
		for (int i = 0; i < HorizontalFlag.Length; i++){
			HorizontalFlag[i].gameObject.SetActive (false);
		}
		for (int i = 0; i < VerticalFlag.Length; i++){
			VerticalFlag[i].gameObject.SetActive (false);
		}
		for (int i = 0; i < SwedishFlag.Length; i++){
			SwedishFlag[i].gameObject.SetActive (false);
		}
		for (int i = 0; i < NorwegianFlag.Length; i++){
			NorwegianFlag[i].gameObject.SetActive (false);
		}
	}

	void AnswerGenerator1(){
		Debug.LogFormat ("[Vexillology #{0}] - - - - -", moduleId);
		Debug.LogFormat ("[Vexillology #{0}] First Colour:", moduleId);
		if (ActiveFlag == 0){
			if (ActiveFlagTop1 == ActiveFlagTop2 && ActiveFlagTop2 == ActiveFlagTop3){
				AnswerColour1 = 7;
				Debug.LogFormat ("[Vexillology #{0}] Three of the same colour present = Black", moduleId);
			} else if ((ActiveFlagTop1 == 1 && ActiveFlagTop2 == 1)||(ActiveFlagTop1 == 1 && ActiveFlagTop3 == 1)||(ActiveFlagTop2 == 1 && ActiveFlagTop3 == 1)){
				AnswerColour1 = 2;
				Debug.LogFormat ("[Vexillology #{0}] Two Oranges present = Green", moduleId);
			} else if ((ActiveFlagTop1 == 2 && ActiveFlagTop2 == 2)||(ActiveFlagTop1 == 2 && ActiveFlagTop3 == 2)||(ActiveFlagTop2 == 2 && ActiveFlagTop3 == 2)){
				AnswerColour1 = 2;
				Debug.LogFormat ("[Vexillology #{0}] Two Greens present = Green", moduleId);
			} else if ((ActiveFlagTop1 == 7 && ActiveFlagTop2 == 7)||(ActiveFlagTop1 == 7 && ActiveFlagTop3 == 7)||(ActiveFlagTop2 == 7 && ActiveFlagTop3 == 7)){
				AnswerColour1 = 2;
				Debug.LogFormat ("[Vexillology #{0}] Two Blacks present = Green", moduleId);
			} else if (ActiveFlagTop3 == 1 || ActiveFlagTop3 == 6){
				AnswerColour1 = 3;
				Debug.LogFormat ("[Vexillology #{0}] Third Position is {1} = Yellow", moduleId, coloursStrings[ActiveFlagTop3]);
			} else if (ActiveFlagTop1 == 2 || ActiveFlagTop1 == 4){
				AnswerColour1 = 1;
				Debug.LogFormat ("[Vexillology #{0}] First Position is {1} = Orange", moduleId, coloursStrings[ActiveFlagTop1]);
			} else if ((ActiveFlagTop1 == 7 || ActiveFlagTop2 == 7 || ActiveFlagTop3 == 7)&&(ActiveFlagTop1 == 5 || ActiveFlagTop2 == 5 || ActiveFlagTop3 == 5)){
				AnswerColour1 = 4;
				Debug.LogFormat ("[Vexillology #{0}] Black and Aqua are present = Blue", moduleId);
			} else if (ActiveFlagTop1 == 3 || ActiveFlagTop2 == 3 || ActiveFlagTop3 == 3){
				AnswerColour1 = 6;
				Debug.LogFormat ("[Vexillology #{0}] Yellow is Present = White", moduleId);
			} else {
				Debug.LogFormat ("[Vexillology #{0}] Otherwise = Red", moduleId);
				AnswerColour1 = 0;
			}
		} else if (ActiveFlag == 1){
			if (ActiveFlagTop1 == 0 && ActiveFlagTop2 == 6 && ActiveFlagTop3 == 4){
				AnswerColour1 = 1;
				Debug.LogFormat ("[Vexillology #{0}] Red, White and Blue are shown in order = Orange", moduleId);
			} else if ((ActiveFlagTop1 == ActiveFlagTop2 && ActiveFlagTop2 == ActiveFlagTop3) || ActiveFlagTop1 == ActiveFlagTop2 || ActiveFlagTop1 == ActiveFlagTop3 || ActiveFlagTop2 == ActiveFlagTop3){
				AnswerColour1 = 7;
				Debug.LogFormat ("[Vexillology #{0}] Two or three of the same colour present = Black", moduleId);
			} else if ((ActiveFlagTop1 == 4 || ActiveFlagTop2 == 4 || ActiveFlagTop3 == 4)&&(ActiveFlagTop1 == 0 || ActiveFlagTop2 == 0 || ActiveFlagTop3 == 0)&&(ActiveFlagTop1 == 3 || ActiveFlagTop2 == 3 || ActiveFlagTop3 == 3)){
				AnswerColour1 = 5;
				Debug.LogFormat ("[Vexillology #{0}] Blue, Red and Yellow are present = Aqua", moduleId);
			} else if (ActiveFlagTop2 == 7 || ActiveFlagTop2 == 1){
				AnswerColour1 = 4;
				Debug.LogFormat ("[Vexillology #{0}] Second Position is {1} = Blue", moduleId, coloursStrings[ActiveFlagTop2]);
			} else if ((ActiveFlagTop1 == 6 || ActiveFlagTop2 == 6 || ActiveFlagTop3 == 6)&&(ActiveFlagTop1 == 5 || ActiveFlagTop2 == 5 || ActiveFlagTop3 == 5)){
				AnswerColour1 = 6;
				Debug.LogFormat ("[Vexillology #{0}] White and Aqua present = White", moduleId);
			} else if (ActiveFlagTop1 == 4 || ActiveFlagTop2 == 4 || ActiveFlagTop3 == 4){
				AnswerColour1 = 2;
				Debug.LogFormat ("[Vexillology #{0}] Blue is present = Green", moduleId);
			} else {
				AnswerColour1 = 0;
				Debug.LogFormat ("[Vexillology #{0}] Otherwise = Red", moduleId);
			}

		} else if (ActiveFlag == 2 || ActiveFlag == 3){
			if (ActiveFlagTop1 == ActiveFlagTop3){
				AnswerColour1 = 3;
				Debug.LogFormat ("[Vexillology #{0}] First and Third colour are the same = Yellow", moduleId);
			} else if ((ActiveFlagTop1 == 3 || ActiveFlagTop2 == 3)&&(ActiveFlagTop2 == 0 || ActiveFlagTop3 == 0)){
				AnswerColour1 = 1;
				Debug.LogFormat ("[Vexillology #{0}] First or second position is Yellow and second or third position is Red = Orange", moduleId);
			} else if ((ActiveFlagTop1 == 6 || ActiveFlagTop2 == 6 || ActiveFlagTop3 == 6)&&(ActiveFlagTop1 == 7 || ActiveFlagTop2 == 7 || ActiveFlagTop3 == 7)){
				AnswerColour1 = 7;
				Debug.LogFormat ("[Vexillology #{0}] Black and White are present = Black", moduleId);
			} else if (ActiveFlagTop3 == 1){
				AnswerColour1 = 2;
				Debug.LogFormat ("[Vexillology #{0}] Third position is Orange = Green", moduleId);
			} else if (ActiveFlagTop2 == 2 || ActiveFlagTop2 == 4){
				AnswerColour1 = 6;
				Debug.LogFormat ("[Vexillology #{0}] Second Position is {1} = White", moduleId, coloursStrings[ActiveFlagTop2]);
			} else if (ActiveFlagTop1 == 0 || ActiveFlagTop2 == 0 || ActiveFlagTop3 == 0){
				AnswerColour1 = 0;
				Debug.LogFormat ("[Vexillology #{0}] Red is present = Red", moduleId);
			} else {
				AnswerColour1 = 4;
				Debug.LogFormat ("[Vexillology #{0}] Otherwise = Blue", moduleId);
			}
		}
	}

	void AnswerGenerator2(){
		Debug.LogFormat ("[Vexillology #{0}] - - - - -", moduleId);
		Debug.LogFormat ("[Vexillology #{0}] Second Colour:", moduleId);
		if (Bomb.GetBatteryCount() == 2 && Bomb.IsIndicatorOn(Indicator.BOB) && Bomb.IsIndicatorOff(Indicator.SIG)){
			AnswerColour2 = 5;
			Debug.LogFormat ("[Vexillology #{0}] 2 batteries, lit BOB and unlit SIG = Aqua", moduleId);
		} else if (Bomb.GetBatteryCount() >= 3 && ActiveFlag == 2){
			Debug.LogFormat ("[Vexillology #{0}] 2-Colour Nordic Cross and 3 or more batteries = Third Colour Ruleset", moduleId);
			SwedishGenerator2();
		} else if (ActiveFlag == 0 && Bomb.GetOnIndicators().Count() >= 2){
			AnswerColour2 = 2;
			Debug.LogFormat ("[Vexillology #{0}] Horizontal Bars and 2 or more lit indicators = Green", moduleId);
		} else if (HasEmptyPortPlate == true){
			AnswerColour2 = 7;
			Debug.LogFormat ("[Vexillology #{0}] Empty portplate present = Black", moduleId);
		} else if (Bomb.IsPortPresent(Port.Parallel) && Bomb.IsPortPresent(Port.StereoRCA)){
			AnswerColour2 = 0;
			Debug.LogFormat ("[Vexillology #{0}] Stereo-RCA and Parallel port present = Red", moduleId);
		} else if (Bomb.GetOffIndicators().Count() > Bomb.GetOnIndicators().Count()){
			AnswerColour2 = 4;
			Debug.LogFormat ("[Vexillology #{0}] More unlit than lit indicators = Blue", moduleId);
		} else if (Bomb.IsPortPresent(Port.Serial)){
			AnswerColour2 = 3;
			Debug.LogFormat ("[Vexillology #{0}] Serial port present = Yellow", moduleId);
		} else {
			AnswerColour2 = 6;
			Debug.LogFormat ("[Vexillology #{0}] Otherwise = White", moduleId);
		}
	}

	void SwedishGenerator2(){
		if (Bomb.GetSerialNumberLetters().Any(coloursStringsCap[ActiveFlagTop1].Contains)){
			colourSerialMatch = true;
			Debug.LogFormat ("[Vexillology #{0}] Serial number does contain letter of first flag pole colour", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Serial number does NOT contain letter of first flag pole colour", moduleId);}

		if (Bomb.IsPortPresent(Port.PS2)){
			PS2Present = true;
			Debug.LogFormat ("[Vexillology #{0}] PS2 port is present", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] PS2 port is NOT present", moduleId);}

		if (Bomb.GetIndicators().Count() >= 2){
			TwoorMoreIncPresent = true;
			Debug.LogFormat ("[Vexillology #{0}] 2 or more indicators present", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Less than 2 indicators present", moduleId);}

		if (Bomb.GetSerialNumberNumbers().Last() >= 5){
			LastDigit5orBigger = true;
			Debug.LogFormat ("[Vexillology #{0}] Last digit of serial number is 5 or higher", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Last digit of serial number is lower than 5", moduleId);}

		if (!colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 0;
		} else if (colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 4;
		} else if (!colourSerialMatch && PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 7;
		} else if (!colourSerialMatch && !PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 1;
		} else if (!colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 6;
		} else if (colourSerialMatch && !PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 6;
		} else if (colourSerialMatch && PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 2;
		} else if (!colourSerialMatch && PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 4;
		} else if (colourSerialMatch && PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 3;
		} else if (colourSerialMatch && PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 1;
		} else if (colourSerialMatch && PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 5;
		} else if (!colourSerialMatch && PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour2 = 0;
		} else if (!colourSerialMatch && PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 4 ;
		} else if (colourSerialMatch && !PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 7;
		} else if (colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 3;
		} else if (!colourSerialMatch && !PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour2 = 2;
		}

		Debug.LogFormat ("[Vexillology #{0}] Second Colour = {1}", moduleId, coloursStrings[AnswerColour2]);

	}

	void AnswerGenerator3(){
		Debug.LogFormat ("[Vexillology #{0}] - - - - -", moduleId);
		Debug.LogFormat ("[Vexillology #{0}] Third Colour:", moduleId);
		if (Bomb.GetSerialNumberLetters().Any(coloursStringsCap[ActiveFlagTop1].Contains)){
			colourSerialMatch = true;
			Debug.LogFormat ("[Vexillology #{0}] Serial number does contain letter of first flag pole colour", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Serial number does NOT contain letter of first flag pole colour", moduleId);}

		if (Bomb.IsPortPresent(Port.PS2)){
			PS2Present = true;
			Debug.LogFormat ("[Vexillology #{0}] PS2 port is present", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] PS2 port is NOT present", moduleId);}

		if (Bomb.GetIndicators().Count() >= 2){
			TwoorMoreIncPresent = true;
			Debug.LogFormat ("[Vexillology #{0}] 2 or more indicators present", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Less than 2 indicators present", moduleId);}

		if (Bomb.GetSerialNumberNumbers().Last() >= 5){
			LastDigit5orBigger = true;
			Debug.LogFormat ("[Vexillology #{0}] Last digit of serial number is 5 or higher", moduleId);
		} else {Debug.LogFormat ("[Vexillology #{0}] Last digit of serial number is lower than 5", moduleId);}

		if (!colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 0;
		} else if (colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 4;
		} else if (!colourSerialMatch && PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 7;
		} else if (!colourSerialMatch && !PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 1;
		} else if (!colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 6;
		} else if (colourSerialMatch && !PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 6;
		} else if (colourSerialMatch && PS2Present && !TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 2;
		} else if (!colourSerialMatch && PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 4;
		} else if (colourSerialMatch && PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 3;
		} else if (colourSerialMatch && PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 1;
		} else if (colourSerialMatch && PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 5;
		} else if (!colourSerialMatch && PS2Present && TwoorMoreIncPresent && !LastDigit5orBigger){
			AnswerColour3 = 0;
		} else if (!colourSerialMatch && PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 4 ;
		} else if (colourSerialMatch && !PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 7;
		} else if (colourSerialMatch && !PS2Present && !TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 3;
		} else if (!colourSerialMatch && !PS2Present && TwoorMoreIncPresent && LastDigit5orBigger){
			AnswerColour3 = 2;
		}

		Debug.LogFormat ("[Vexillology #{0}] Third Colour = {1}", moduleId, coloursStrings[AnswerColour3]);

	}

	void TransposingCheck(){
		Debug.LogFormat ("[Vexillology #{0}] - - - - -", moduleId);
		Debug.LogFormat ("[Vexillology #{0}] Transposing:", moduleId);

		if ((ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 0)||(ActiveFlag == 1 && AnswerColour1 == 2 && AnswerColour2 == 6 && AnswerColour3 == 2)||(ActiveFlag == 1 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 0)){
			Debug.LogFormat ("[Vexillology #{0}] Flag is an Country. No transposing needed", moduleId);
		}

		else if (AnswerColour1 != AnswerColour2 && AnswerColour1 != AnswerColour3 && AnswerColour2 != AnswerColour3){
			Debug.LogFormat ("[Vexillology #{0}] No duplicate colours. No transposing needed", moduleId);
		} else {
			while (AnswerColour1 == AnswerColour2 || AnswerColour1 == AnswerColour3 || AnswerColour2 == AnswerColour3){
				if (AnswerColour1 == AnswerColour2){
					TransposeSecond();
				} else if (AnswerColour1 == AnswerColour3 || AnswerColour3 == AnswerColour2){
					TransposeThird();
				}
			}
		}
	}

	void TransposeSecond(){
		int OldAnswerColour2 = AnswerColour2;
		if (AnswerColour2 == 0){
			if (AnswerColour1 != 6){
				AnswerColour2 = 6;
			} else if (AnswerColour1 != 5){
				AnswerColour2 = 5;
			} else {AnswerColour2 = 2;}
		} else if (AnswerColour2 == 1){
			if (AnswerColour1 != 0){
				AnswerColour2 = 0;
			} else if (AnswerColour1 != 4){
				AnswerColour2 = 4;
			} else {AnswerColour2 = 5;}
		} else if (AnswerColour2 == 3){
			if (AnswerColour1 != 4){
				AnswerColour2 = 4;
			} else if (AnswerColour1 != 0){
				AnswerColour2 = 0;
			} else {AnswerColour2 = 1;}
		} else if (AnswerColour2 == 2){
			if (AnswerColour1 != 3){
				AnswerColour2 = 3;
			} else if (AnswerColour1 != 7){
				AnswerColour2 = 7;
			} else {AnswerColour2 = 6;}
		} else if (AnswerColour2 == 4){
			if (AnswerColour1 != 7){
				AnswerColour2 = 7;
			} else if (AnswerColour1 != 3){
				AnswerColour2 = 3;
			} else {AnswerColour2 = 0;}
		} else if (AnswerColour2 == 5){
			if (AnswerColour1 != 1){
				AnswerColour2 = 1;
			} else if (AnswerColour1 != 2){
				AnswerColour2 = 2;
			} else {AnswerColour2 = 7;}
		} else if (AnswerColour2 == 6){
			if (AnswerColour1 != 2){
				AnswerColour2 = 2;
			} else if (AnswerColour1 != 1){
				AnswerColour2 = 1;
			} else {AnswerColour2 = 4;}
		} else if (AnswerColour2 == 7){
			if (AnswerColour1 != 5){
				AnswerColour2 = 5;
			} else if (AnswerColour1 != 6){
				AnswerColour2 = 6;
			} else {AnswerColour2 = 3;}
		}
		Debug.LogFormat ("[Vexillology #{0}] Transposed the second colour from {1} to {2}", moduleId, coloursStrings[OldAnswerColour2], coloursStrings[AnswerColour2]);
	}

	void TransposeThird(){
		int OldAnswerColour3 = AnswerColour3;
		if (AnswerColour3 == 0){
			if (AnswerColour1 != 6 && AnswerColour2 != 6){
				AnswerColour3 = 6;
			} else if (AnswerColour1 != 5 && AnswerColour2 != 5){
				AnswerColour3 = 5;
			} else {AnswerColour3 = 2;}
		} else if (AnswerColour3 == 1){
			if (AnswerColour1 != 0 && AnswerColour2 != 0){
				AnswerColour3 = 0;
			} else if (AnswerColour1 != 4 && AnswerColour2 != 4){
				AnswerColour3 = 4;
			} else {AnswerColour3 = 5;}
		} else if (AnswerColour3 == 3){
			if (AnswerColour1 != 4 && AnswerColour2 != 4){
				AnswerColour3 = 4;
			} else if (AnswerColour1 != 0 && AnswerColour2 != 0){
				AnswerColour3 = 0;
			} else {AnswerColour3 = 1;}
		} else if (AnswerColour3 == 2){
			if (AnswerColour1 != 3 && AnswerColour2 != 3){
				AnswerColour3 = 3;
			} else if (AnswerColour1 != 7 && AnswerColour2 != 7){
				AnswerColour3 = 7;
			} else {AnswerColour3 = 6;}
		} else if (AnswerColour3 == 4){
			if (AnswerColour1 != 7 && AnswerColour2 != 7){
				AnswerColour3 = 7;
			} else if (AnswerColour1 != 3 && AnswerColour2 != 3){
				AnswerColour3 = 3;
			} else {AnswerColour3 = 0;}
		} else if (AnswerColour3 == 5){
			if (AnswerColour1 != 1 && AnswerColour2 != 1){
				AnswerColour3 = 1;
			} else if (AnswerColour1 != 2 && AnswerColour2 != 2){
				AnswerColour3 = 2;
			} else {AnswerColour3 = 7;}
		} else if (AnswerColour3 == 6){
			if (AnswerColour1 != 2 && AnswerColour2 != 2){
				AnswerColour3 = 2;
			} else if (AnswerColour1 != 1 && AnswerColour2 != 1){
				AnswerColour3 = 1;
			} else {AnswerColour3 = 4;}
		} else if (AnswerColour3 == 7){
			if (AnswerColour1 != 5 && AnswerColour2 != 5){
				AnswerColour3 = 5;
			} else if (AnswerColour1 != 6 && AnswerColour2 != 6){
				AnswerColour3 = 6;
			} else {AnswerColour3 = 3;}
		}
		Debug.LogFormat ("[Vexillology #{0}] Transposed the third colour from {1} to {2}", moduleId, coloursStrings[OldAnswerColour3], coloursStrings[AnswerColour3]);
	}
	
	void CheckCountries(){
		if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 4 && AnswerColour3 == 1){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Armenia", moduleId);
			SubmitTime = 4;
			AnthemNumber = 1;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Austria", moduleId);
			SubmitTime = 3;
			AnthemNumber = 2;
		} else if (ActiveFlag == 1 && AnswerColour1 == 7 && AnswerColour2 == 3 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Belgium", moduleId);
			SubmitTime = 2;
			AnthemNumber = 3;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 3 && AnswerColour3 == 2){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Bolivia", moduleId);
			SubmitTime = 1;
			AnthemNumber = 4;
		} else if (ActiveFlag == 0 && AnswerColour1 == 6 && AnswerColour2 == 2 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Bulgaria", moduleId);
			SubmitTime = 9;
			AnthemNumber = 5;
		} else if (ActiveFlag == 1 && AnswerColour1 == 4 && AnswerColour2 == 3 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Chad and Romania", moduleId);
			_ChadRomania = true;
			AnthemNumber = 30;
		} else if (ActiveFlag == 2 && AnswerColour1 == 0 && AnswerColour2 == 6){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Denmark", moduleId);
			SubmitTime = 5;
			AnthemNumber = 6;
		}  else if (ActiveFlag == 0 && AnswerColour1 == 4 && AnswerColour2 == 7 && AnswerColour3 == 6){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Estonia", moduleId);
			SubmitTime = 2;
			AnthemNumber = 7;
		} else if (ActiveFlag == 3 && AnswerColour1 == 6 && AnswerColour2 == 0 && AnswerColour3 == 4){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of the Faroe Islands", moduleId);
			SubmitTime = 8;
			AnthemNumber = 8;
		} else if (ActiveFlag == 2 && AnswerColour1 == 6 && AnswerColour2 == 4){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Finland", moduleId);
			SubmitTime = 8;
			AnthemNumber = 9;
		} else if (ActiveFlag == 1 && AnswerColour1 == 4 && AnswerColour2 == 6 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of France", moduleId);
			SubmitTime = 3;
			AnthemNumber = 10;
		} else if (ActiveFlag == 0 && AnswerColour1 == 2 && AnswerColour2 == 3 && AnswerColour3 == 4){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Gabon", moduleId);
			SubmitTime = 1;
			AnthemNumber = 11;
		} else if (ActiveFlag == 0 && AnswerColour1 == 7 && AnswerColour2 == 0 && AnswerColour3 == 3){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Germany", moduleId);
			SubmitTime = 9;
			AnthemNumber = 12;
		} else if (ActiveFlag == 1 && AnswerColour1 == 0 && AnswerColour2 == 3 && AnswerColour3 == 2){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Guinea", moduleId);
			SubmitTime = 4;
			AnthemNumber = 13;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 2){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Hungary", moduleId);
			SubmitTime = 6;
			AnthemNumber = 14;
		} else if (ActiveFlag == 3 && AnswerColour1 == 4 && AnswerColour2 == 0 && AnswerColour3 == 6){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Iceland", moduleId);
			SubmitTime = 4;
			AnthemNumber = 15;
		} else if (ActiveFlag == 1 && AnswerColour1 == 2 && AnswerColour2 == 6 && AnswerColour3 == 1){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Ireland", moduleId);
			SubmitTime = 3;
			AnthemNumber = 16;
		} else if (ActiveFlag == 1 && AnswerColour1 == 2 && AnswerColour2 == 6 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Italy", moduleId);
			SubmitTime = 9;
			AnthemNumber = 17;
		} else if (ActiveFlag == 1 && AnswerColour1 == 1 && AnswerColour2 == 6 && AnswerColour3 == 2){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Ivory Coast", moduleId);
			SubmitTime = 5;
			AnthemNumber = 18;
		} else if (ActiveFlag == 0 && AnswerColour1 == 3 && AnswerColour2 == 2 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Lithuania", moduleId);
			SubmitTime = 0;
			AnthemNumber = 19;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 5){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Luxembourg", moduleId);
			SubmitTime = 2;
			AnthemNumber = 20;
		} else if (ActiveFlag == 1 && AnswerColour1 == 2 && AnswerColour2 == 3 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Mali", moduleId);
			SubmitTime = 3;
			AnthemNumber = 21;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 4){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of The Netherlands", moduleId);
			SubmitTime = 1;
			AnthemNumber = 22;
		} else if (ActiveFlag == 1 && AnswerColour1 == 2 && AnswerColour2 == 6 && AnswerColour3 == 2){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Nigeria", moduleId);
			SubmitTime = 6;
			AnthemNumber = 23;
		} else if (ActiveFlag == 3 && AnswerColour1 == 0 && AnswerColour2 == 4 && AnswerColour3 == 6){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Norway", moduleId);
			SubmitTime = 7;
			AnthemNumber = 24;
		} else if (ActiveFlag == 1 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Peru", moduleId);
			SubmitTime = 1;
			AnthemNumber = 25;
		} else if (ActiveFlag == 0 && AnswerColour1 == 6 && AnswerColour2 == 4 && AnswerColour3 == 0){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Russia", moduleId);
			SubmitTime = 7;
			AnthemNumber = 26;
		} else if (ActiveFlag == 0 && AnswerColour1 == 2 && AnswerColour2 == 6 && AnswerColour3 == 4){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Sierra Leone", moduleId);
			SubmitTime = 2;
			AnthemNumber = 27;
		} else if (ActiveFlag == 2 && AnswerColour1 == 4 && AnswerColour2 == 3){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Sweden", moduleId);
			SubmitTime = 6;
			AnthemNumber = 28;
		} else if (ActiveFlag == 0 && AnswerColour1 == 0 && AnswerColour2 == 6 && AnswerColour3 == 7){
			Debug.LogFormat ("[Vexillology #{0}] Answer is the flag of Yemen", moduleId);
			SubmitTime = 7;
			AnthemNumber = 29;
		}
	}

	void PressedColour(int i){
		ActiveColour = i;
	}

	void PressedHorzFlag(int i){
		FlagPartColourHorz[i].material = butColour[ActiveColour];
		if (i == 0){
			FlagColour1 = ActiveColour;
		} else if (i == 1){
			FlagColour2 = ActiveColour;
		} else {
			FlagColour3 = ActiveColour;
		}
	}

	void PressedVertFlag(int i){
		FlagPartColourVert[i].material = butColour[ActiveColour];
		if (i == 0){
			FlagColour1 = ActiveColour;
		} else if (i == 1){
			FlagColour2 = ActiveColour;
		} else {
			FlagColour3 = ActiveColour;
		}
	}

	void PressedSweFlag(int i){
		if (i <= 3){
			FlagColour1 = ActiveColour;
			FlagPartColourSwe[0].material = butColour[ActiveColour];
			FlagPartColourSwe[1].material = butColour[ActiveColour];
			FlagPartColourSwe[2].material = butColour[ActiveColour];
			FlagPartColourSwe[3].material = butColour[ActiveColour];
		} else if (i > 3){
			FlagColour2 = ActiveColour;
			FlagPartColourSwe[4].material = butColour[ActiveColour];
			FlagPartColourSwe[5].material = butColour[ActiveColour];
		}
	}

	void PressedNorFlag(int i){
		if (i <= 3){
			FlagColour1 = ActiveColour;
			FlagPartColourNor[0].material = butColour[ActiveColour];
			FlagPartColourNor[1].material = butColour[ActiveColour];
			FlagPartColourNor[2].material = butColour[ActiveColour];
			FlagPartColourNor[3].material = butColour[ActiveColour];
		} else if (i == 4 || i == 5){
			FlagColour2 = ActiveColour;
			FlagPartColourNor[4].material = butColour[ActiveColour];
			FlagPartColourNor[5].material = butColour[ActiveColour];
		}	else if (i > 5){
			FlagColour3 = ActiveColour;
			FlagPartColourNor[6].material = butColour[ActiveColour];
			FlagPartColourNor[7].material = butColour[ActiveColour];
			FlagPartColourNor[8].material = butColour[ActiveColour];
			FlagPartColourNor[9].material = butColour[ActiveColour];
			FlagPartColourNor[10].material = butColour[ActiveColour];
			FlagPartColourNor[11].material = butColour[ActiveColour];
			FlagPartColourNor[12].material = butColour[ActiveColour];
			FlagPartColourNor[13].material = butColour[ActiveColour];
		}
	}

	void PressedSubmit(){
		string Time = Bomb.GetFormattedTime();
		float Time2 = Bomb.GetTime();
		string TimeX = Time.Remove(2);

		if (!_issolved){
			Debug.LogFormat ("[Vexillology #{0}] = = = = = = = = =", moduleId);
			if (ActiveFlag == 2 && FlagColour1 != 10 && FlagColour2 != 10){
				Debug.LogFormat ("[Vexillology #{0}] The submitted flag was {1} and {2} at {3}", moduleId, coloursStrings[FlagColour1], coloursStrings[FlagColour2], Time.ToString());
				if (SubmitTime != 10){
					Debug.LogFormat ("[Vexillology #{0}] Submit Time must include {1}", moduleId, SubmitTime.ToString());
					if (FlagColour1 == AnswerColour1 && FlagColour2 == AnswerColour2 && ((Time2 <= 60 && TimeX.Contains(SubmitTime.ToString())) || (Time2 > 60 && Time.Contains(SubmitTime.ToString())))){
						if (AnthemNumber == 100){
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						} else {
							Audio.PlaySoundAtTransform(anthems[AnthemNumber-1].name, transform);
						}
						GetComponent<KMBombModule>().HandlePass();
						_issolved = true;
						Debug.LogFormat ("[Vexillology #{0}] Module Solved!", moduleId);
					} else {
						GetComponent<KMBombModule>().HandleStrike();
						if (FlagColour1 != AnswerColour1 || FlagColour2 != AnswerColour2){
							Debug.LogFormat ("[Vexillology #{0}] Strike! Colours were wrong", moduleId);
						} else {
							Debug.LogFormat ("[Vexillology #{0}] Strike! Submitted while correct number wasn't present on timer", moduleId);
						}
					}
				} else {
					Debug.LogFormat ("[Vexillology #{0}] Submit Time must include {1}", moduleId, Bomb.GetSerialNumberNumbers().Last().ToString());
					if (FlagColour1 == AnswerColour1 && FlagColour2 == AnswerColour2 && ((Time2 <= 60 && TimeX.Contains( Bomb.GetSerialNumberNumbers().Last().ToString())) || (Time2 > 60 &&Time.Contains( Bomb.GetSerialNumberNumbers().Last().ToString())))){
						if (AnthemNumber == 100){
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						} else {
							Audio.PlaySoundAtTransform(anthems[AnthemNumber-1].name, transform);
						}
						GetComponent<KMBombModule>().HandlePass();
						_issolved = true;
						Debug.LogFormat ("[Vexillology #{0}] Module Solved!", moduleId);
					} else {
					GetComponent<KMBombModule>().HandleStrike();
					if (FlagColour1 != AnswerColour1 || FlagColour2 != AnswerColour2){
							Debug.LogFormat ("[Vexillology #{0}] Strike! Colours were wrong", moduleId);
						} else {
							Debug.LogFormat ("[Vexillology #{0}] Strike! Submitted while correct number wasn't present on timer", moduleId);
						}
					}
				}
			} else if (FlagColour1 != 10 && FlagColour2 != 10 && FlagColour3 != 10){
				Debug.LogFormat ("[Vexillology #{0}] The submitted flag was {1}, {2} and {3} at {4}", moduleId, coloursStrings[FlagColour1], coloursStrings[FlagColour2], coloursStrings[FlagColour3], Time.ToString());
				if (_ChadRomania){
					Debug.LogFormat ("[Vexillology #{0}] Submit Time must include 0 or 5", moduleId);
					if (FlagColour1 == AnswerColour1 && FlagColour2 == AnswerColour2 && FlagColour3 == AnswerColour3 && ((Time2 <= 60 && (Time.Contains("5") || Time.Contains("0")) || (Time2 > 60 && (Time.Contains("5") || Time.Contains("0")))))){
						if (AnthemNumber == 100){
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						} else {
							AnthemNumber = AnthemNumber - UnityEngine.Random.Range(0, 2);
							Audio.PlaySoundAtTransform(anthems[AnthemNumber].name, transform);
						}
						GetComponent<KMBombModule>().HandlePass();
						_issolved = true;
						Debug.LogFormat ("[Vexillology #{0}] Module Solved!", moduleId);
					} else {
						GetComponent<KMBombModule>().HandleStrike();
						if (FlagColour1 != AnswerColour1 || FlagColour2 != AnswerColour2 || FlagColour3 != AnswerColour3){
							Debug.LogFormat ("[Vexillology #{0}] Strike! Colours were wrong", moduleId);
						} else {
							Debug.LogFormat ("[Vexillology #{0}] Strike! Submitted while correct number wasn't present on timer", moduleId);
						}
					}
				} else if (SubmitTime != 10){
					Debug.LogFormat ("[Vexillology #{0}] Submit Time must include {1}", moduleId, SubmitTime.ToString());
					if (FlagColour1 == AnswerColour1 && FlagColour2 == AnswerColour2 && FlagColour3 == AnswerColour3 && ((Time2 <= 60 && TimeX.Contains(SubmitTime.ToString())) || (Time2 > 60 && Time.Contains(SubmitTime.ToString())))){
						if (AnthemNumber == 100){
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						} else {
							Audio.PlaySoundAtTransform(anthems[AnthemNumber-1].name, transform);
						}
						GetComponent<KMBombModule>().HandlePass();
						_issolved = true;
						Debug.LogFormat ("[Vexillology #{0}] Module Solved!", moduleId);
					} else {
						GetComponent<KMBombModule>().HandleStrike();
						if (FlagColour1 != AnswerColour1 || FlagColour2 != AnswerColour2 || FlagColour3 != AnswerColour3){
							Debug.LogFormat ("[Vexillology #{0}] Strike! Colours were wrong", moduleId);
						} else {
							Debug.LogFormat ("[Vexillology #{0}] Strike! Submitted while correct number wasn't present on timer", moduleId);
						}
					}
				} else {
					Debug.LogFormat ("[Vexillology #{0}] Submit Time must include {1}", moduleId, Bomb.GetSerialNumberNumbers().Last().ToString());
					if (FlagColour1 == AnswerColour1 && FlagColour2 == AnswerColour2 && FlagColour3 == AnswerColour3 && ((Time2 <= 60 && TimeX.Contains( Bomb.GetSerialNumberNumbers().Last().ToString())) || (Time2 > 60 &&Time.Contains( Bomb.GetSerialNumberNumbers().Last().ToString())))){
						if (AnthemNumber == 100){
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						} else {
							Audio.PlaySoundAtTransform(anthems[AnthemNumber-1].name, transform);
						}
						GetComponent<KMBombModule>().HandlePass();
						_issolved = true;
						Debug.LogFormat ("[Vexillology #{0}] Module Solved!", moduleId);
					} else {
					GetComponent<KMBombModule>().HandleStrike();
					if (FlagColour1 != AnswerColour1 || FlagColour2 != AnswerColour2 || FlagColour3 != AnswerColour3){
							Debug.LogFormat ("[Vexillology #{0}] Strike! Colours were wrong", moduleId);
						} else {
							Debug.LogFormat ("[Vexillology #{0}] Strike! Submitted while correct number wasn't present on timer", moduleId);
						}
					}
				}
			} else {
				Debug.LogFormat ("[Vexillology #{0}] Strike! Grey Colours present.", moduleId);
				GetComponent<KMBombModule>().HandleStrike();
			}
		}
	}

	private IEnumerator FlagTopCirulator(){
		while(!_issolved){
			FlagTopSubmitRen.material = butColour[ActiveFlagTop1];
			yield return new WaitForSeconds(.5f);
			FlagTopSubmitRen.material = butColour[8];
			yield return new WaitForSeconds(.5f);
			FlagTopSubmitRen.material = butColour[ActiveFlagTop2];
			yield return new WaitForSeconds(.5f);
			FlagTopSubmitRen.material = butColour[8];
			yield return new WaitForSeconds(.5f);
			FlagTopSubmitRen.material = butColour[ActiveFlagTop3];
			yield return new WaitForSeconds(.5f);
			FlagTopSubmitRen.material = butColour[8];
			yield return new WaitForSeconds(3f);
		}
		FlagTopSubmitRen.material = butColour[8];
	}

	#pragma warning disable 414
	private string TwitchHelpMessage = @"Submit the flag with “!{0} submit on 5”. Fill a colour of the flag with “!{0} fill colour 1 red” or “!{0} fill 3 yellow”.";
	#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string inputCommand)
	{
		int final = 0;
		Regex rgx1 = new Regex(@"^(press|submit) (at|on|with) [0-9]$");
		Regex rgx2 = new Regex(@"^(fill) (colour|color) [1-3] (red|orange|yellow|green|blue|aqua|white|black)$");
		Regex rgx3 = new Regex(@"^(fill) [1-3] (red|orange|yellow|green|blue|aqua|white|black)$");
		if (rgx2.IsMatch(inputCommand)) 
		{
			var commands = inputCommand.ToLowerInvariant().Split(new[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);

			if (commands[3]=="red"){
				ActiveColour = 0;
			} else if (commands[3]=="orange"){
				ActiveColour = 1;
			} else if (commands[3]=="green"){
				ActiveColour = 2;
			} else if (commands[3]=="yellow"){
				ActiveColour = 3;
			} else if (commands[3]=="blue"){
				ActiveColour = 4;
			} else if (commands[3]=="aqua"){
				ActiveColour = 5;
			} else if (commands[3]=="white"){
				ActiveColour = 6;
			} else if (commands[3]=="black"){
				ActiveColour = 7;
			}
			
			string result = commands [2];

			if (ActiveFlag == 0)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					HorizontalFlag [finalII].OnInteract();
					yield return new WaitForSeconds(.1f);
				}
			} 
			else if (ActiveFlag == 1)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					VerticalFlag [finalII].OnInteract();
					yield return new WaitForSeconds(.1f);
				}
			} 
			else if (ActiveFlag == 2)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;

					if (final == 1){
						SwedishFlag [finalII].OnInteract();
					} else {
						finalII = 4;
						SwedishFlag [finalII].OnInteract();
					}
					yield return new WaitForSeconds(.1f);
				}
			} 
			else
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					if (final == 1){
						NorwegianFlag [finalII].OnInteract();
					} else if (final == 2){
						finalII = 4;
						NorwegianFlag [finalII].OnInteract();
					} else {
						finalII = 6;
						NorwegianFlag [finalII].OnInteract();
					}
					yield return new WaitForSeconds(.1f);
				}
			}
		} else if (rgx3.IsMatch(inputCommand)) 
		{
			var commands = inputCommand.ToLowerInvariant().Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

			if (commands[2]=="red"){
				ActiveColour = 0;
			} else if (commands[2]=="orange"){
				ActiveColour = 1;
			} else if (commands[2]=="green"){
				ActiveColour = 2;
			} else if (commands[2]=="yellow"){
				ActiveColour = 3;
			} else if (commands[2]=="blue"){
				ActiveColour = 4;
			} else if (commands[2]=="aqua"){
				ActiveColour = 5;
			} else if (commands[2]=="white"){
				ActiveColour = 6;
			} else if (commands[2]=="black"){
				ActiveColour = 7;
			}
			
			string result = commands [1];

			if (ActiveFlag == 0)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					HorizontalFlag [finalII].OnInteract();
					yield return new WaitForSeconds(.1f);
				}
			} 
			else if (ActiveFlag == 1)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					VerticalFlag [finalII].OnInteract();
					yield return new WaitForSeconds(.1f);
				}
			} 
			else if (ActiveFlag == 2)
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;

					if (final == 1){
						SwedishFlag [finalII].OnInteract();
					} else {
						finalII = 4;
						SwedishFlag [finalII].OnInteract();
					}
					yield return new WaitForSeconds(.1f);
				}
			} 
			else
			{
				if (Int32.TryParse(result, out final))
				{
					yield return null;
					int finalII = final - 1;
					if (final == 1){
						NorwegianFlag [finalII].OnInteract();
					} else if (final == 2){
						finalII = 4;
						NorwegianFlag [finalII].OnInteract();
					} else {
						finalII = 6;
						NorwegianFlag [finalII].OnInteract();
					}
					yield return new WaitForSeconds(.1f);
				}
			}
		}
		else if (rgx1.IsMatch(inputCommand))
		{
			int Time2 = (int)Math.Floor(Bomb.GetTime());
			string Time = Bomb.GetFormattedTime();
			string TimeX = Time.Remove(2);
			var commands = inputCommand.ToLowerInvariant().Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

			while ((!Bomb.GetFormattedTime().Contains(commands[2])) && (Time2 > 60))
			{
				Time2 = (int)Math.Floor(Bomb.GetTime());
				yield return new WaitForSeconds(.01f);
			}

			if (Bomb.GetFormattedTime().Contains(commands[2]) && (Time2 > 60))
			{
				FlagTopSubmit.OnInteract();
			}

			while ((!Bomb.GetFormattedTime().Remove(2).Contains(commands[2])) && (Time2 <= 60))
			{
				Time2 = (int)Math.Floor(Bomb.GetTime());
				yield return new WaitForSeconds(.01f);
			}

			if ((Bomb.GetFormattedTime().Remove(2).Contains(commands[2])) && (Time2 <= 60))
			{
				yield return null;
				FlagTopSubmit.OnInteract();
				yield return new WaitForSeconds(.1f);
			}

			yield return null;
		}
	}
}
