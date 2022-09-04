using ABI_RC.Core.Networking.IO.UserGeneratedContent;
using ABI_RC.Core.Player;
using ABI_RC.Systems.MovementSystem;
using System.Collections.Generic;
using UnityEngine;

namespace ml_amt
{
    class MotionTweaker : MonoBehaviour
    {
        enum ParameterType
        {
            Upright
        }
        enum ParameterSyncType
        {
            Local,
            Synced
        }

        struct AdditionalParameterInfo
        {
            public ParameterType m_type;
            public ParameterSyncType m_sync;
            public string m_name;
            public int m_hash; // For local only
        }

        static readonly Vector4 ms_pointVector = new Vector4(0f, 0f, 0f, 1f);

        RootMotion.FinalIK.VRIK m_vrIk = null;

        bool m_ready = false;

        bool m_standing = true;
        bool m_prone = false;
        float m_currentUpright = 1f;
        float m_locomotionWeight = 1f;
        float m_crouchLimit = 0.65f;
        float m_proneLimit = 0.35f;
        bool m_customCrouchLimit = false;
        bool m_customProneLimit = false;

        readonly List<AdditionalParameterInfo> m_parameters = null;

        public MotionTweaker()
        {
            m_parameters = new List<AdditionalParameterInfo>();
        }

        void Start()
        {
            if(PlayerSetup.Instance._inVr)
                PlayerSetup.Instance.avatarSetupCompleted.AddListener(this.OnAvatarSetup);
        }

        void Update()
        {
            if(m_ready)
            {
                // Update upright
                Matrix4x4 l_hmdMatrix = PlayerSetup.Instance.transform.GetMatrix().inverse * (PlayerSetup.Instance._inVr ? PlayerSetup.Instance.vrHeadTracker.transform.GetMatrix() : PlayerSetup.Instance.desktopCameraRig.transform.GetMatrix());
                float l_currentHeight = Mathf.Clamp((l_hmdMatrix * ms_pointVector).y, 0f, float.MaxValue);
                float l_avatarViewHeight = Mathf.Clamp(PlayerSetup.Instance.GetViewPointHeight() * PlayerSetup.Instance._avatar.transform.localScale.y, 0f, float.MaxValue);
                m_currentUpright = Mathf.Clamp((((l_currentHeight > 0f) && (l_avatarViewHeight > 0f)) ? (l_currentHeight / l_avatarViewHeight) : 0f), 0f, 1f);
                m_standing = (m_currentUpright > m_crouchLimit);
                m_prone = (m_currentUpright < m_proneLimit);

                if((m_vrIk != null) && m_vrIk.enabled && !PlayerSetup.Instance._movementSystem.sitting && !PlayerSetup.Instance.fullBodyActive)
                {
                    float immobileWeight = (PlayerSetup.Instance._movementSystem.movementVector.magnitude <= Mathf.Epsilon) ? 1f : 0f;
                    float standingWeight = m_standing ? 1f : 0f;
                    float flyingWeight = PlayerSetup.Instance._movementSystem.flying ? 0f : 1f;
                    m_locomotionWeight = standingWeight * immobileWeight * flyingWeight;
                    m_vrIk.solver.locomotion.weight = m_locomotionWeight;

                    // Immediately update avatar position if locomotion is off
                    if (m_locomotionWeight <= Mathf.Epsilon) {
                        Transform headTransform = PlayerSetup.Instance._animator.GetBoneTransform(HumanBodyBones.Head);
                        if (headTransform != null)
                        {
                            Vector3 position = headTransform.position;
                            {
                                PlayerSetup.Instance._avatar.transform.position = new Vector3(
                                    position.x,
                                    PlayerSetup.Instance._avatar.transform.position.y,
                                    position.z);
                                headTransform.position = position;
                            }
                        }
                    }

                    if (!m_standing)
                    {
                        if (m_prone)
                        {
                            MovementSystem.Instance.ChangeCrouch(false);
                            MovementSystem.Instance.ChangeProne(true);
                            PlayerSetup.Instance._animator.SetBool("Crouching", false);
                            PlayerSetup.Instance._animator.SetBool("Prone", true);
                        }
                        else
                        {
                            MovementSystem.Instance.ChangeCrouch(true);
                            MovementSystem.Instance.ChangeProne(false);
                            PlayerSetup.Instance._animator.SetBool("Crouching", true);
                            PlayerSetup.Instance._animator.SetBool("Prone", false);
                        }
                    } 
                    else
                    {
                        MovementSystem.Instance.ChangeCrouch(false);
                        MovementSystem.Instance.ChangeProne(false);
                        PlayerSetup.Instance._animator.SetBool("Crouching", false);
                        PlayerSetup.Instance._animator.SetBool("Prone", false);
                    }
                }

                if(m_parameters.Count > 0)
                {
                    foreach(AdditionalParameterInfo l_param in m_parameters)
                    {
                        switch(l_param.m_type)
                        {
                            case ParameterType.Upright:
                            {
                                switch(l_param.m_sync)
                                {
                                    case ParameterSyncType.Local:
                                        PlayerSetup.Instance._animator.SetFloat(l_param.m_hash, m_currentUpright);
                                        break;
                                    case ParameterSyncType.Synced:
                                        PlayerSetup.Instance.changeAnimatorParam(l_param.m_name, m_currentUpright);
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        public void OnAvatarClear()
        {
            m_vrIk = null;
            m_ready = false;
            m_standing = true;
            m_parameters.Clear();
            m_locomotionWeight = 1f;
            m_crouchLimit = 0.65f;
            m_proneLimit = 0.35f;
            m_customCrouchLimit = false;
        }

        public void OnAvatarSetup()
        {
            m_vrIk = PlayerSetup.Instance._avatar.GetComponent<RootMotion.FinalIK.VRIK>();

            // Parse animator parameters
            AnimatorControllerParameter[] l_params = PlayerSetup.Instance._animator.parameters;
            ParameterType[] l_enumParams = (ParameterType[])System.Enum.GetValues(typeof(ParameterType));

            foreach(var l_param in l_params)
            {
                foreach(var l_enumParam in l_enumParams)
                {
                    if(l_param.name.Contains(l_enumParam.ToString()) && (m_parameters.FindIndex(p => p.m_type == l_enumParam) == -1))
                    {
                        bool l_local = (l_param.name[0] == '#');

                        m_parameters.Add(new AdditionalParameterInfo
                        {
                            m_type = l_enumParam,
                            m_sync = (l_local ? ParameterSyncType.Local : ParameterSyncType.Synced),
                            m_name = l_param.name,
                            m_hash = (l_local ? l_param.nameHash : 0)
                        });

                        break;
                    }
                }
            }

            Transform l_customCrouchLimit = PlayerSetup.Instance._avatar.transform.Find("CrouchLimit");
            Transform l_customProneLimit = PlayerSetup.Instance._avatar.transform.Find("ProneLimit");
            m_customCrouchLimit = (l_customCrouchLimit != null);
            m_crouchLimit = m_customCrouchLimit ? Mathf.Clamp(l_customCrouchLimit.localPosition.y, 0f, 1f) : Settings.CrouchLimit;
            m_customProneLimit = (l_customProneLimit != null);
            m_proneLimit = m_customProneLimit ? Mathf.Clamp(l_customProneLimit.localPosition.y, 0f, 1f) : Settings.ProneLimit;

            m_ready = true;
        }

        public void SetCrouchLimit(float p_value)
        {
            if(!m_customCrouchLimit)
                m_crouchLimit = Mathf.Clamp(p_value, 0f, 1f);
        }

        public void SetProneLimit(float p_value)
        {
            if (!m_customProneLimit)
                m_proneLimit = Mathf.Clamp(p_value, 0f, 1f);
        }
    }
}
