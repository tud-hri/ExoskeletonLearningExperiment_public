using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{

	public class UXF_Generator : MonoBehaviour
	{
		[SerializeField] private ConditionOrder _conditionOrder;
		
		Dictionary<string, System.Action> stageSetupActions = new Dictionary<string, System.Action>();

		// we will call this when the session starts.
		public void Generate(Session session)
		{
			int.TryParse(session.ppid, out var pNum);
			_conditionOrder.SetParticipantNumber(pNum);
			Debug.Log($"participant number is { pNum }");

			// Get the condition
			var condition = _conditionOrder.GetCurrentCondition();
			if (condition != null) session.participantDetails["condition"] = condition;
			else Debug.LogWarning("Can't find a condition. Make sure you correctly set the ConditionOrder Asset.");
			Debug.Log($"The condition is: {condition}");

			// Set up the Dictionary with functionality based on the stages in the experiment - to make things a little more maintainable
			stageSetupActions.Add("familiarization", SetupFamiliarization);
			stageSetupActions.Add("baseline", SetupBaseline);
			stageSetupActions.Add("training", SetupTraining);
			stageSetupActions.Add("final", SetupFinal);
			stageSetupActions.Add("resting", SetupResting);
			stageSetupActions.Add("retention", SetupRetention);

			// Set up the session blocks
			var stages = session.settings.GetStringList("stages");
			foreach (var stage in stages)
			{
				stageSetupActions[stage].Invoke();
			}

            stageSetupActions["training"].Invoke(); // Just add a training block to the end of the session to make sure we don't end prematurely...
        }
		
		void SetupFamiliarization() => CreateBlock(1, Session.instance.settings.GetInt("familiarization_time"), "familiarization");
		void SetupBaseline() => CreateBlock(1, Session.instance.settings.GetInt("trial_time"), "baseline",nQuestionnaires:1);
		void SetupFinal() => CreateBlock(1, Session.instance.settings.GetInt("trial_time"), "final",nQuestionnaires:1);
		void SetupRetention() => CreateBlock(1, Session.instance.settings.GetInt("trial_time"), "retention");
		void SetupResting() => CreateBlock(1, Session.instance.settings.GetInt("resting_time"), "resting");

		void SetupTraining() // Set up the scene for Training, here, they can just repeat the steps and train to get the timing down, without being scored
		{
			Session session = Session.instance; 
			
			// Set up the block
			var nTrials = session.settings.GetInt("training_trials");
			var block = CreateBlock(nTrials, session.settings.GetInt("training_time"), "training");

			// Set the cues & perspective based on condition for this participant
			if (session.participantDetails["condition"] == null)
			{
				Debug.LogWarning("Could not find a condition set, setting to NF condition.");
				session.participantDetails["condition"] = "NF";
			}

			SetConditionSettings(block.settings, (string)session.participantDetails["condition"]);

            block.settings.SetValue("nQuestionnaires", 4); // set the AMOUNT of questionnaires that will be used in this block
		}

		///////////////////////// Helper methods
		private void SetConditionSettings(Settings settings, string condition)
		{
			switch (condition)
			{
				case "NF":
					settings.SetValue("cues_enabled", false);
					settings.SetValue("perspective", Perspective.FirstPerson);
					break;
				case "NT":
					settings.SetValue("cues_enabled", false);
					settings.SetValue("perspective", Perspective.Lateral);
					break;
				case "VF":
					settings.SetValue("cues_enabled", true);
					settings.SetValue("perspective", Perspective.FirstPerson);
					break;
				case "VT":
					settings.SetValue("cues_enabled", true);
					settings.SetValue("perspective", Perspective.Lateral);
					break;
				default:
					Debug.LogWarning($"ATTENTION: invalid condition { condition } found.");
					break;
			}
		}
		
		private Block CreateBlock(int nTrials, int runtime, string stageName, int nQuestionnaires = -1)
		{
			Block block = Session.instance.CreateBlock(nTrials);
			
			block.settings.SetValue("runtime", runtime);
			block.settings.SetValue("stage_name", stageName);
            block.settings.SetValue("nQuestionnaires", nQuestionnaires); // set the AMOUNT of questionnaires that will be used in this block

			SetConditionSettings(block.settings, "NF");

            return block;
		}

	}
}
