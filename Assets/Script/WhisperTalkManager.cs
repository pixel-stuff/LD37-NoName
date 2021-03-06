﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WhisperTalkManager : MonoBehaviour {

	public TextMesh m_text;
	public GameObject m_container;
	public GameObject m_arrow;
	public int m_tickAlive = 3;
	private int m_tickBeforeErase = 0;
	private bool m_isDisplay = false;
	private Quaternion m_startContainerRotation;
	private Quaternion m_startTextRotation;

	public Action m_tickDisplayOver;
	// Use this for initialization
	void Awake () {
		m_startContainerRotation = m_container.transform.localRotation;
		m_startTextRotation = m_text.transform.localRotation;
		StopDisplayWhisper ();
		TimeManager.OnTicTriggered += TickHappen;
	}

	public void StartDisplayWhisper(string txt, bool displayOnRight = true){
		if (txt.Length >= 20) {
			try {
				m_text.text = txt.Insert (txt.IndexOf (" ", txt.Length / 2), "\n");
			} catch (Exception e) {
				m_text.text = txt;
			}
		} else {
			m_text.text = txt;
		}
		m_tickBeforeErase = m_tickAlive;
		m_container.SetActive (true);
		m_text.color = new Color (0f, 0f, 0f, 0f);
		m_isDisplay = true;
		if (!displayOnRight) {
			m_container.transform.localRotation = Quaternion.identity;
			m_text.transform.localRotation = Quaternion.identity;
			m_container.transform.localPosition = new Vector3 (-4.18f,0.39f,0.0f);
			m_arrow.transform.localPosition = new Vector3(1.06f,-0.09f,0.0f);
		} else {
			m_container.transform.localPosition = new Vector3 (-1.31f,0.39f,0.0f);
			m_container.transform.localRotation = Quaternion.identity;
			m_text.transform.localRotation = Quaternion.identity;
			m_arrow.transform.localPosition = new Vector3(-1.06f,-0.09f,0.0f);

			//m_container.transform.rotation = m_startContainerRotation;
			//m_text.transform.rotation = Quaternion.Euler(0,180,0); 
		}
		this.GetComponent<Animation> ().Play ("WhisperFadeIn");
	}

	public void StopDisplayWhisper(){
		this.GetComponent<Animation> ().Play ("WhisperFadeOut");
		m_text.text = "";
		//m_container.SetActive (false);
		m_isDisplay = false;
	}

	public void TickHappen(GameTime gt){
		if (m_isDisplay) {
			m_tickBeforeErase--;
			if (m_tickBeforeErase <= 0) {
				StopDisplayWhisper ();
				if (m_tickDisplayOver != null) {
					m_tickDisplayOver ();
				}
			}
		}
	}

	public void OnDestroy(){
		TimeManager.OnTicTriggered -= TickHappen;
	}

	public void DisplayTextFromAnimation(){
		m_text.color = Color.black;
	}

	public void RemoveTextFromAnimation(){
		m_container.SetActive (false);
		m_text.color = new Color (1f, 1f, 1f, 0f);
	}
}
