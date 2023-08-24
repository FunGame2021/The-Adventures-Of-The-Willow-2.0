using FMOD.Studio;
using System;
using System.Data;
using UnityEngine;
using static AudioManager;

public class SnapshotManager : MonoBehaviour
{
    public SnapshotType snapshotType = SnapshotType.Normal;

    private EventInstance snapshotInstance;

    [SerializeField] private string actualSnapshot;

    private void Start()
    {
        SetSnapshot(snapshotType);
        if (!string.IsNullOrEmpty(actualSnapshot))
        {
            snapshotType = (SnapshotType)Enum.Parse(typeof(SnapshotType), actualSnapshot);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetSnapshot(snapshotType);
            actualSnapshot = snapshotType.ToString();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetSnapshot(SnapshotType.Normal);
            actualSnapshot = snapshotType.ToString();
        }
    }

    private void SetSnapshot(SnapshotType type)
    {
        snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        switch (type)
        {
            case SnapshotType.Normal:
                snapshotInstance = SnapshotReferences.NormalSnapshot;
                break;
            case SnapshotType.Cave:
                snapshotInstance = SnapshotReferences.CaveSnapshot;
                break;
            case SnapshotType.Underwater:
                snapshotInstance = SnapshotReferences.UnderwaterSnapshot;
                break;
        }

        snapshotInstance.start();
    }

}

public enum SnapshotType
{
    Normal,
    Cave,
    Underwater
}
