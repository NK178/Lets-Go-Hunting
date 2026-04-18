using System;
using System.Collections.Generic;
using UnityEngine;


//18/4/2026
//For now, they will act independently, but I guess ideally they sync up with the section manager 
public class DriveTrack : MonoBehaviour
{

    //[SerializeField] private List<DriveCheckpoint> inputCheckpointList;
    [SerializeField] private DriveCheckpoint[] inputCheckpoints;

    private List<DriveCheckpoint> checkpoints;

    private int currentCheckpointIndex = 0;
    private bool hasCompletedTrack = false;
    private bool isActive = false;


    //For now, this wil do 
    public static Action<DriveCheckpoint> onCheckpointSet; 


    //private DriveCheckpoint targetCheckpoint; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        DriveCheckpoint[] tempArray = new DriveCheckpoint[inputCheckpoints.Length];
        checkpoints = new List<DriveCheckpoint>();
        for (int i = 0; i < inputCheckpoints.Length; i++)
        {
            tempArray[i] = inputCheckpoints[i];
        }
        MergeSort(tempArray, 0, inputCheckpoints.Length - 1);
        for (int i = 0; i < tempArray.Length; i++)
        {
            checkpoints.Add(tempArray[i]);
        }

        currentCheckpointIndex = 0;
        //targetCheckpoint = checkpoints[0];
    }

    // Update is called once per frame
    void Update()
    {

        //if (targetCheckpoint == null)
        //    return;

        if (hasCompletedTrack || !isActive)
            return; 


        bool hasReachedCheckpoint = checkpoints[currentCheckpointIndex].IsDetected();
        if (hasReachedCheckpoint)
        {
            if (currentCheckpointIndex < inputCheckpoints.Length)
            {
                if (currentCheckpointIndex + 1 == inputCheckpoints.Length)
                    hasCompletedTrack = true;
                else
                {
                    currentCheckpointIndex++;
                    onCheckpointSet.Invoke(checkpoints[currentCheckpointIndex]);
                }
            }
        }
    }

    public DriveCheckpoint GetTargetCheckpoint()
    {
        if (currentCheckpointIndex + 1 == inputCheckpoints.Length && hasCompletedTrack)
            return null;
        else 
            return checkpoints[currentCheckpointIndex]; 
    }

    public void RecieveSignal(GAMESIGNAL gameSignal)
    {
        string signal = gameSignal.ToString();

        if (signal.Contains("START"))
        {

            Debug.Log("STARTING AUTO DRIVE");
            isActive = true;
            onCheckpointSet.Invoke(checkpoints[currentCheckpointIndex]);
        }
        else if (signal.Contains("END"))
        {
            isActive = false;
        }


    }


    private void ChangeCheckpoint()
    {

    }


    //lmao should make a helper function class or something 
    private void MergeSort(DriveCheckpoint[] array, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;

            MergeSort(array, left, mid);
            MergeSort(array, mid + 1, right);
            Merge(array, left, mid, right);
        }
    }

    private void Merge(DriveCheckpoint[] array, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        DriveCheckpoint[] list1 = new DriveCheckpoint[n1];
        DriveCheckpoint[] list2 = new DriveCheckpoint[n2];
        int i = 0;
        int j = 0;

        for (i = 0; i < n1; i++)
        {
            list1[i] = array[left + i];
        }
        for (j = 0; j < n2; j++)
        {
            list2[j] = array[mid + 1 + j];
        }

        int k = mid;
        while (i < n1 && j < n2)
        {
            int lIndex = list1[i].GetOrderIndex();
            int rIndex = list2[i].GetOrderIndex();

            if (lIndex <= rIndex)
            {
                array[k] = list1[i];
                i++;
            }
            else
            {
                array[k] = list2[j];
                j++;
            }
            k++;
        }

        while (i < n1)
        {
            array[k] = list1[i];
            i++;
            k++;
        }
        while (j < n2)
        {
            array[k] = list2[j];
            j++;
            k++;
        }
    }

}
