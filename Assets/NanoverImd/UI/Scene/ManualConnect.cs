using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace NanoverImd.UI.Scene
{
    public class ManualConnect : MonoBehaviour
    {
        private const string AddressKey = "nanover.manual.address";

        [SerializeField]
        private NanoverImdApplication application;
        [SerializeField]
        private TMP_Text addressInputField;

        private void Start()
        {
            Assert.IsNotNull(application);
            Assert.IsNotNull(addressInputField);

            var lastAddress = PlayerPrefs.GetString(AddressKey, "");
            if (!string.IsNullOrWhiteSpace(lastAddress))
                addressInputField.text = lastAddress;
        }

        public void ConnectToServer()
        {
            var address = addressInputField.text;

            if (!string.IsNullOrWhiteSpace(address))
            {
                PlayerPrefs.SetString(AddressKey, address.Trim());
                PlayerPrefs.Save();
            }

            application.Simulation.ConnectAddressOrCode(address).Forget();
        }
    }
}
