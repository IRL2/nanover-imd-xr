using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace NanoverImd.UI.Scene
{
    /// <summary>
    /// Connects to a server address or code typed by the user, and remembers
    /// it so it is filled in next time the app starts.
    /// </summary>
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

        /// <summary>
        /// Called from the UI button to tell the application to connect
        /// to remote services.
        /// </summary>
        public void ConnectToServer()
        {
            var address = addressInputField.text;

            if (!string.IsNullOrWhiteSpace(address))
            {
                PlayerPrefs.SetString(AddressKey, address.Trim());
                // Save now: a headset app is often closed without quitting.
                PlayerPrefs.Save();
            }

            application.Simulation.ConnectAddressOrCode(address).Forget();
        }
    }
}
