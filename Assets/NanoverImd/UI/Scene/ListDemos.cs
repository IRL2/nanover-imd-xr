using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Nanover.Frontend.UI;
using UnityEngine;
using UnityEngine.Events;

namespace NanoverImd.UI.Scene
{
    public class ListDemos : MonoBehaviour
    {
        [SerializeField]
        private List<NanoverRecordings.DemoListing> demos;

        [SerializeField]
        private NanoverImdApplication application;

        [SerializeField]
        private DynamicMenu menu;

        [SerializeField]
        private Sprite demoIcon;

        [SerializeField]
        private UnityEvent startSearch;

        [SerializeField]
        private UnityEvent endSearch;


        private void OnEnable()
        {
            startSearch?.Invoke();
            Refresh();
        }

        public void Refresh()
        {
            NanoverRecordings
                .FetchDemosListing()
                .AsUniTask()
                .ContinueWith((listing) =>
            {
                demos = listing;
                RefreshDemos();
            });
        }

        public void RefreshDemos()
        {
            menu.ClearChildren();
            foreach (var demo in demos)
            {
                menu.AddItem(
                    demo.Name, 
                    demoIcon, 
                    () => NanoverRecordings.LoadDemo(demo.URL).AsUniTask().ContinueWith(application.Simulation.ConnectRecordingReader)
                );
            }
            endSearch?.Invoke ();
        }
    }
}