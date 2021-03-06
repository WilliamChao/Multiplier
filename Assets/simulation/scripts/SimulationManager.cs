﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Extension;
using Common;
using MultiPlayer;
using SinglePlayer;

namespace Simulation {
	public struct EquationInfo {
		public string health;
		public string attack;
		public string speed;
		public string split;
		public string merge;
		public string attackCooldown;
	}

	public class SimulationManager : MonoBehaviour {
		//Constants
		public const byte INVALID = 0xFF;
		public const byte LARGER = 0xF0;
		public const byte APPROXIMATE = 0x00;
		public const byte SMALLER = 0x0F;

		//Definite flag to let user modify either Yellow or Blue.
		public bool isEditingYellowTeam; //TRUE: Yellow Team. FALSE: Blue Team. Default value is Yellow.

		//Toggle Groups
		public ToggleGroup editTeamToggleGroup;
		public ToggleGroup unitAttributeToggleGroup;

		//Equation Editor stuffs
		public Text labelName;
		public InputField equationInputField;
		public RectTransform contentPane;
		public GameObject levelInfoPrefab;
		public EquationInfo[] teamEquations;

		//AI Attribute Managers
		public AIAttributeManager yellowTeamAttributes;
		public AIAttributeManager blueTeamAttributes;

		//Leaderboards
		public Leaderboard yellowTeamLeaderboard;
		public Leaderboard blueTeamLeaderboard;

		//Simulation Starter
		public SimulationStarter simulationStarter;

		public void Start() {
			Initialization();
		}

		public void FixedUpdate() {
			CheckTeamToggle();
			CheckAttributeToggle();
		}

		public void UpdateAttribute(Toggle toggle) {
			if (!toggle.isOn) {
				return;
			}

			int index = this.isEditingYellowTeam ? 0 : 1;
			string description = "Enter equation. Current: (";
			EnumToggle enumToggle = toggle.GetComponent<EnumToggle>();
			if (enumToggle != null) {
				switch (enumToggle.value) {
					default:
						Debug.LogError("Invalid toggle value.");
						break;
					case 0: //Health
						this.equationInputField.text = description + this.teamEquations[index].health + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].health);
						break;
					case 1: //Attack
						this.equationInputField.text = description + this.teamEquations[index].attack + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].attack);
						break;
					case 2: //Speed
						this.equationInputField.text = description + this.teamEquations[index].speed + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].speed);
						break;
					case 3: //Split
						this.equationInputField.text = description + this.teamEquations[index].split + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].split);
						break;
					case 4: //Merge
						this.equationInputField.text = description + this.teamEquations[index].merge + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].merge);
						break;
					case 5: //Attack Cooldown
						this.equationInputField.text = description + this.teamEquations[index].attackCooldown + ")";
						UpdateLevelInfo(enumToggle.value, this.teamEquations[index].attackCooldown);
						break;
				}
			}
		}

		public void UpdateTeamToggle(Toggle toggle) {
			if (!toggle.isOn) {
				return;
			}

			EnumToggle enumToggle = toggle.GetComponent<EnumToggle>();
			if (enumToggle == null) {
				Debug.Log("Unable to obtain EnumToggle component. Did you accidentally passed in the wrong Toggle?");
				return;
			}

			this.isEditingYellowTeam = enumToggle.value == 0 ? true : false;

			//NOTE(Thompson): If enumToggle.value is not using 0 or 1, change this to check on isEditingYellowTeam.
			int index = enumToggle.value;
			Toggle attributeToggle = this.unitAttributeToggleGroup.GetSingleActiveToggle();
			if (attributeToggle != null) {
				EnumToggle attributeEnumToggle = attributeToggle.GetComponent<EnumToggle>();
				if (attributeEnumToggle != null) {
					string equationValue = "Enter equation. Current: (";
					switch (attributeEnumToggle.value) {
						default:
							Debug.LogError("Attribute Enum Toggle value is invalid.");
							equationValue = "[INTERNAL ERROR]: Invalid attribute enum toggle value.";
							break;
						case 0:
							equationValue += this.teamEquations[index].health + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].health);
							break;
						case 1:
							equationValue += this.teamEquations[index].attack + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].attack);
							break;
						case 2:
							equationValue += this.teamEquations[index].speed + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].speed);
							break;
						case 3:
							equationValue += this.teamEquations[index].split + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].split);
							break;
						case 4:
							equationValue += this.teamEquations[index].merge + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].merge);
							break;
						case 5:
							equationValue += this.teamEquations[index].attackCooldown + ")";
							UpdateLevelInfo(attributeEnumToggle.value, this.teamEquations[index].attackCooldown);
							break;
					}
					this.equationInputField.text = equationValue;
				}
			}
		}

		public void UpdateEquation() {
			try {

				string equation = this.equationInputField.text;
				Debug.Log("Equation is: " + equation);

				AIAttributeManager AIManager = null;
				int index;
				if (this.isEditingYellowTeam) {
					AIManager = this.yellowTeamAttributes;
					index = 0;
				}
				else {
					AIManager = this.blueTeamAttributes;
					index = 1;
				}

				Toggle attributeToggle = this.unitAttributeToggleGroup.GetSingleActiveToggle();
				if (attributeToggle != null) {
					EnumToggle toggle = attributeToggle.GetComponent<EnumToggle>();
					if (toggle != null) {
						switch (toggle.value) {
							default:
								Debug.LogError("Wrong toggle value: " + toggle.value + ". Please check.");
								return;
							case 0:
								AIManager.SetDirectHealthAttribute(equation);
								this.teamEquations[index].health = equation;
								break;
							case 1:
								AIManager.SetDirectAttackAttribute(equation);
								this.teamEquations[index].attack = equation;
								break;
							case 2:
								AIManager.SetDirectSpeedAttribute(equation);
								this.teamEquations[index].speed = equation;
								break;
							case 3:
								AIManager.SetDirectSplitAttribute(equation);
								this.teamEquations[index].split = equation;
								break;
							case 4:
								AIManager.SetDirectMergeAttribute(equation);
								this.teamEquations[index].merge = equation;
								break;
							case 5:
								AIManager.SetDirectAttackCooldownAttribute(equation);
								this.teamEquations[index].attackCooldown = equation;
								break;
						}

						UpdateLevelInfo(toggle.value, equation);
					}
				}
				this.simulationStarter.StopSimulation();
				this.simulationStarter.ClearSimulation();
				this.simulationStarter.InitializeSimulation();
			}
			catch (System.Exception) {
				this.equationInputField.text = "[Invalid Equation.]";
			}
		}

		public void UpdateLevelInfo(int toggleValue, string equation) {
			try {
				float previousAnswer = 0f;
				float answer = 0f;
				for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
					switch (toggleValue) {
						default:
							Debug.LogError("Invalid toggle value: " + toggleValue + ". Please check.");
							return;
						case 0:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.Health, i+1, i, previousAnswer);
							break;
						case 1:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.Attack, i + 1, i, previousAnswer);
							break;
						case 2:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.Speed, i + 1, i, previousAnswer);
							break;
						case 3:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.Split, i + 1, i, previousAnswer);
							break;
						case 4:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.Merge, i + 1, i, previousAnswer);
							break;
						case 5:
							answer = (float)MathParser.ProcessEquation(equation, AttributeProperty.AttackCooldown, i + 1, i, previousAnswer);
							break;
					}
					UpdateLevelInfoIteration(i, answer, previousAnswer);
					previousAnswer = answer;
				}
			}
			catch (System.Exception) {
				this.equationInputField.text = "[Invalid Equation.]";
			}
		}

		//-----------------  PRIVATE METHODS  ----------------------------

		private void UpdateLevelInfoIteration(int level, float answer, float previousAnswer) {
			bool isLevelInfoInitialized = false;
			GameObject obj = null;
			if (this.contentPane.transform.childCount < Attributes.MAX_NUM_OF_LEVELS) {
				obj = MonoBehaviour.Instantiate(this.levelInfoPrefab);
				isLevelInfoInitialized = true;
			}
			else {
				obj = this.contentPane.GetChild(level).gameObject;
			}
			LevelInfo levelInfo = obj.GetComponent<LevelInfo>();
			levelInfo.level = level + 1;
			levelInfo.rate = answer;
			if (levelInfo.level == 1) {
				levelInfo.comparisonFlag = INVALID;
			}
			else {
				if (answer < previousAnswer) {
					levelInfo.comparisonFlag = SMALLER;
				}
				else if (Mathf.Abs(previousAnswer - answer) <= float.Epsilon) {
					levelInfo.comparisonFlag = APPROXIMATE;
				}
				else if (answer > previousAnswer) {
					levelInfo.comparisonFlag = LARGER;
				}
			}

			levelInfo.UpdateText();

			if (isLevelInfoInitialized) {
				levelInfo.transform.SetParent(this.contentPane.transform);
				levelInfo.transform.position = this.contentPane.transform.position;
				RectTransform rectTransform = levelInfo.GetComponent<RectTransform>();
				rectTransform.localScale = Vector3.one;
				rectTransform.localRotation = this.contentPane.localRotation;
			}
		}

		private void Initialization() {
			//GameObjects null checking. Yes, you can do bitwise operations.
			bool flag = this.editTeamToggleGroup == null;
			flag |= this.unitAttributeToggleGroup == null;
			flag |= this.equationInputField == null;
			flag |= this.levelInfoPrefab == null;
			flag |= this.simulationStarter == null;
			flag |= this.contentPane == null;
			if (flag) {
				Debug.LogError("One of the game objects is null. Please check.");
			}

			//Boolean flags
			this.isEditingYellowTeam = true;

			//Array initialization
			this.teamEquations = new EquationInfo[2];
			InitializeTeamEquations();

			//Initialize content pane.
			SetDefaultLevelInfo();
		}

		private void CheckTeamToggle() {
			Toggle editTeamToggle = this.editTeamToggleGroup.GetSingleActiveToggle();
			if (editTeamToggle != null) {
				EnumToggle toggle = editTeamToggle.GetComponent<EnumToggle>();
				if (toggle != null) {
					this.isEditingYellowTeam = toggle.value == 0 ? true : false;
				}
			}
		}

		private void CheckAttributeToggle() {
			Toggle attributeToggle = this.unitAttributeToggleGroup.GetSingleActiveToggle();
			if (attributeToggle != null) {
				EnumToggle toggle = attributeToggle.GetComponent<EnumToggle>();
				if (toggle != null) {
					string label;
					switch (toggle.value) {
						default:
							label = "Error";
							break;
						case 0:
							label = "Health";
							break;
						case 1:
							label = "Attack";
							break;
						case 2:
							label = "Speed";
							break;
						case 3:
							label = "Split";
							break;
						case 4:
							label = "Merge";
							break;
						case 5:
							label = "Attack Cooldown";
							break;
					}
					this.labelName.text = label;
				}
			}
		}

		private void SetDefaultLevelInfo() {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				GameObject obj = null;
				bool sameObjectCheck = false;
				if (this.contentPane.transform.childCount < Attributes.MAX_NUM_OF_LEVELS) {
					obj = MonoBehaviour.Instantiate(this.levelInfoPrefab);
				}
				else {
					obj = this.contentPane.GetChild(i).gameObject;
					sameObjectCheck = true;
				}
				LevelInfo levelInfo = obj.GetComponent<LevelInfo>();
				levelInfo.level = i + 1;
				levelInfo.rate = 1f;
				levelInfo.comparisonFlag = INVALID;
				levelInfo.UpdateText();

				levelInfo.transform.SetParent(this.contentPane.transform);
				RectTransform rectTransform = levelInfo.GetComponent<RectTransform>();
				Vector3 pos;
				if (sameObjectCheck) {
					pos = this.contentPane.GetChild(i).GetComponent<RectTransform>().localPosition;
				}
				else {
					pos = this.contentPane.transform.position;
				}
				pos.z = 0f;
				rectTransform.localScale = Vector3.one;
				rectTransform.localRotation = this.contentPane.localRotation;
				rectTransform.localPosition = pos;
			}

			string defaultEquation = "y=1";
			this.yellowTeamAttributes.SetDirectHealthAttribute(defaultEquation);
			this.yellowTeamAttributes.SetDirectAttackAttribute(defaultEquation);
			this.yellowTeamAttributes.SetDirectSpeedAttribute(defaultEquation);
			this.yellowTeamAttributes.SetDirectSplitAttribute(defaultEquation);
			this.yellowTeamAttributes.SetDirectMergeAttribute(defaultEquation);
			this.yellowTeamAttributes.SetDirectAttackCooldownAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectHealthAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectAttackAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectSpeedAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectSplitAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectMergeAttribute(defaultEquation);
			this.blueTeamAttributes.SetDirectAttackCooldownAttribute(defaultEquation);
		}

		private void InitializeTeamEquations() {
			string defaultEquation = "y=1";
			for (int i = 0; i < this.teamEquations.Length; i++) {
				this.teamEquations[i].health = defaultEquation;
				this.teamEquations[i].attack = defaultEquation;
				this.teamEquations[i].speed = defaultEquation;
				this.teamEquations[i].split = defaultEquation;
				this.teamEquations[i].merge = defaultEquation;
				this.teamEquations[i].attackCooldown = defaultEquation;
			}
		}
	}
}
