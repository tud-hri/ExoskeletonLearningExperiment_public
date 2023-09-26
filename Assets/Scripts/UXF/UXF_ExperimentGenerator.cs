using System.Collections.Generic;
using UnityEngine;

namespace UXF
{
	public class UXF_ExperimentGenerator : MonoBehaviour
	{
		public void Generate(Session session)
		{
			var stages = session.settings.GetStringList("stages", new List<string>(){"pre-test"});
			var stage_times = session.settings.GetStringList("stage_times", new List<string>(){"10m"});

			// create two blocks
			Block[] blocks = new Block[stages.Count];
			
			Block block1 = session.CreateBlock(1);
			Block block2 = session.CreateBlock(1);

			// for each trial in the session, 50/50 chance of correct target being on left or right
			foreach (Trial trial in session.Trials)
			{
				//TargetPosition pos = Random.value > 0.5 ? TargetPosition.Left : TargetPosition.Right;
				//trial.settings.SetValue("correct_target_position", pos);
			}

			// set the block to be inverted ("go to the opposite target") or not, depending on the participant
			bool invertedBlockFirst;

			try
			{
				invertedBlockFirst = (bool) session.participantDetails["inverted_block_first"];
			}
			catch (System.NullReferenceException)
			{
				// during quick start mode, there are no participant details, so we get null reference exception
				invertedBlockFirst = Random.value > 0.5;
				Debug.LogFormat("Inverted block first: {0}", invertedBlockFirst);
			}	
			catch (KeyNotFoundException)
			{
				// during quick start mode, there are no participant details, so we get null reference exception
				invertedBlockFirst = Random.value > 0.5;
				Debug.LogFormat("Inverted block first: {0}", invertedBlockFirst);
			}			

		
			if (invertedBlockFirst)
			{
				block1.settings.SetValue("inverted", true);
				block2.settings.SetValue("inverted", false);
			}
			else
			{
				block1.settings.SetValue("inverted", false);
				block2.settings.SetValue("inverted", true);
			}

		}

		private void StageToBlock()
		{
			
		}
	}
}