using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VRQuestionnaireToolkit.StudySetup))]
public class SetConditionForQ : MonoBehaviour
{
    private VRQuestionnaireToolkit.StudySetup studySetup;

    private void Start()
    {
        studySetup = GetComponent<VRQuestionnaireToolkit.StudySetup>();
    }

    private void SetId() { studySetup.ParticipantId = UXF.Session.instance.ppid; }
    private void SetCondition() { studySetup.Condition = UXF.Session.instance.CurrentBlock.settings.GetString("stage_name"); }
}
