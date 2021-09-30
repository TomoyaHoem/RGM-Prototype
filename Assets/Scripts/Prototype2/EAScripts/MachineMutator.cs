using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineMutator : MonoBehaviour
{
    delegate void MutationDelegate(List<GameObject> mSegs, int index);
    delegate bool TryMutationDelegate(List<GameObject> mSegs, int index);

    GameObject newSegment;
    GameObject mRef;

    SegmentSelectionLogic sL;

    int mutationSuccess, possibleMutations;
    List<int> possibleList;
    List<int> succList;

    private System.Random random = new System.Random();

    private void Awake()
    {
        sL = gameObject.AddComponent<SegmentSelectionLogic>();

        possibleList = new List<int>();
        succList = new List<int>();

        for (int i = 0; i < 4; i++)
        {
            possibleList.Add(0);
            succList.Add(0);
        }
    }

    public IEnumerator MutateMachines(List<GameObject> population, bool feas)
    {
        mutationSuccess = 0;
        possibleMutations = 0;
        for (int i = 0; i < 4; i++)
        {
            possibleList[i] = 0;
            succList[i] = 0;
        }

        int feasindex = SettingsReader.Instance.EASettings.FitFunc.Count - 1;

        foreach (GameObject machine in population)
        {
            float random = Random.Range(0f, 1f);
            if (random < SettingsReader.Instance.EASettings.MutationRate)
            {
                possibleMutations++;
                AssignMutation(machine);
            }
        }

        UIStatistics.Instance.MutationChance = (float)mutationSuccess / possibleMutations;

        int index = feas ? 0 : 1;
        for (int i = 0; i < 4; i++)
        {
            SettingsReader.Instance.MachineSettings.EAStatistics[16 + (2 * i) + index].Add(possibleList[i]);
            SettingsReader.Instance.MachineSettings.EAStatistics[35 + (2 * i) + index].Add(succList[i]);
        }

        yield return null;
    }

    private void AssignMutation(GameObject machine)
    {
        mRef = machine;

        if (random.NextDouble() < SettingsReader.Instance.EASettings.MutationRate || true)
        {
            int method = Random.Range(0, 4);
            method = 3;

            switch (method)
            {
                case 0:
                    possibleList[0]++;
                    Mutate(machine, DeleteSegment, TryDeleteSegment);
                    break;
                case 1:
                    possibleList[1]++;
                    Mutate(machine, AddSegment, TryAddSegment);
                    break;
                case 2:
                    possibleList[2]++;
                    Mutate(machine, ChangeSegment, TryChangeSegment);
                    break;
                default:
                    possibleList[3]++;
                    Mutate(machine, ReplaceSegment, TryReplaceSegment);
                    break;
            }
        }
    }

    //delete/add/change/replace segment at random index
    private void Mutate(GameObject machine, MutationDelegate mutationDelegate, TryMutationDelegate tryMutationDelegate)
    {
        List<GameObject> mSegs = machine.GetComponent<Machine>().Segments;

        int index = Random.Range(0, mSegs.Count);
        index = 2;
        //Debug.Log("mutating: " + machine.name + " at: " + index + " -> " + tryMutationDelegate.Method.Name);

        //disable latter half of machine
        SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), false);
        Vector2 offset;
        //check if new segment fits
        if (tryMutationDelegate(mSegs, index))
        {
            //if yes calculate offset via output of new segment, if deleting set output to input
            //if last segment dont calculate offset
            offset = (index + 1) < mSegs.Count ? newSegment.GetComponent<SegmentPart>().Output - mSegs[index + 1].GetComponent<SegmentPart>().Input : Vector2.zero;
            //Debug.Log(offset);
            //check if rest of machine still fits after
            if ((index + 1) >= mSegs.Count)
            {
                //if yes add/replace new segment, move rest and reactivate
                mutationDelegate(mSegs, index);
                MoveMachine(mSegs.GetRange(index, mSegs.Count - index), offset);
                SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
                mutationSuccess++;
                //Debug.Log("mutation last successfull");
                return;
            }
            else
            {
                //check if output direction matches, if yes -> check collision else check mirrored
                //Debug.Log(newSegment.GetComponent<SegmentPart>().OutputDirection.x + " " + mSegs[index + 1].GetComponent<SegmentPart>().InputDirection.x);
                if (newSegment.GetComponent<SegmentPart>().OutputDirection.x == mSegs[index + 1].GetComponent<SegmentPart>().InputDirection.x)
                {
                    if (CheckCollision(mSegs.GetRange(index + 1, mSegs.Count - index - 1), offset))
                    {
                        //if yes add/replace new segment, move rest and reactivate
                        mutationDelegate(mSegs, index);
                        MoveMachine(mSegs.GetRange(index, mSegs.Count - index), offset);
                        SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
                        mutationSuccess++;
                        //Debug.Log("mutation normal successfull");
                        return;
                    }
                    //Debug.Log("fail");
                }
                else
                {
                    if (CheckCollisionMirrored(mSegs.GetRange(index + 1, mSegs.Count - index - 1), offset))
                    {
                        //if yes add/replace new segment, move rest and reactivate
                        mutationDelegate(mSegs, index);
                        MoveMachineMirrored(mSegs.GetRange(index, mSegs.Count - index), offset);
                        SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
                        mutationSuccess++;
                        //Debug.Log("mutation mirrored successfull");
                        return;
                    }
                    //Debug.Log("mirr fail");
                }
            }
        }

        //cant fit -> reactivate machine, destroy new segment, and go back to original
        //Debug.Log("newly generated does not fit");
        if (mSegs.Contains(newSegment))
        {
            mSegs.Remove(newSegment);
        }
        Destroy(newSegment);
        SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
    }

    private bool TryDeleteSegment(List<GameObject> mSegs, int index)
    {
        if (mSegs.Count < 2) return false;
        newSegment = Instantiate(mSegs[index]);
        newSegment.GetComponent<SegmentPart>().Output = mSegs[index].GetComponent<SegmentPart>().Input;
        newSegment.GetComponent<SegmentPart>().OutputDirection = mSegs[index].GetComponent<SegmentPart>().OutputDirection;
        return true;
    }

    private bool TryAddSegment(List<GameObject> mSegs, int index)
    {
        //generate random segment ID, only try domino or balltrack because of mill bug
        int segID = Random.Range(0, sL.NumPossibleSegments);
        //Debug.Log(segID);
        GenerateNewSegment(mSegs, index, segID);
        //if it has enough room addto list, so that all segments prior get checked including old at index
        if (sL.CheckSegmentRoom(newSegment))
        {
            newSegment.GetComponent<SegmentLogic>().GenerateSegment();
            mSegs.Insert(index, newSegment);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryChangeSegment(List<GameObject> mSegs, int index)
    {
        //get same segment ID as index
        int segID = mSegs[index].GetComponent<SegmentPart>().SegmentID;
        //generate new segment with same ID
        GenerateNewSegment(mSegs, index, segID);
        if (sL.CheckSegmentRoom(newSegment))
        {
            newSegment.GetComponent<SegmentLogic>().GenerateSegment();
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryReplaceSegment(List<GameObject> mSegs, int index)
    {
        //generate random segment ID, only try domino or balltrack
        int segID = Random.Range(0, sL.NumPossibleSegments);
        //generate new random segment
        GenerateNewSegment(mSegs, index, segID);
        if (sL.CheckSegmentRoom(newSegment))
        {
            newSegment.GetComponent<SegmentLogic>().GenerateSegment();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void DeleteSegment(List<GameObject> mSegs, int index)
    {
        Destroy(newSegment);
        Destroy(mSegs[index]);
        mSegs.RemoveAt(index);

        succList[0]++;
    }

    private void AddSegment(List<GameObject> mSegs, int index)
    {
        //parent new segment and set sibling index to replaced
        newSegment.transform.parent = mRef.transform;
        newSegment.transform.SetSiblingIndex(mSegs[index + 1].transform.GetSiblingIndex());

        succList[1]++;
    }

    private void ChangeSegment(List<GameObject> mSegs, int index)
    {
        //parent new segment and set sibling index to replaced
        newSegment.transform.parent = mRef.transform;
        newSegment.transform.SetSiblingIndex(mSegs[index].transform.GetSiblingIndex());
        Destroy(mSegs[index]);
        mSegs.RemoveAt(index);
        mSegs.Insert(index, newSegment);

        succList[2]++;
    }

    private void ReplaceSegment(List<GameObject> mSegs, int index)
    {
        //parent new segment and set sibling index to replaced
        newSegment.transform.parent = mRef.transform;
        newSegment.transform.SetSiblingIndex(mSegs[index].transform.GetSiblingIndex());
        Destroy(mSegs[index]);
        mSegs.RemoveAt(index);
        mSegs.Insert(index, newSegment);

        succList[3]++;
    }

    private void GenerateNewSegment(List<GameObject> mSegs, int index, int segID)
    {
        newSegment = new GameObject("SegmentNew");
        sL.AssignSegment(newSegment, segID);
        sL.SetSegmentIO(newSegment, mSegs[index].GetComponent<SegmentPart>().Input, mSegs[index].GetComponent<SegmentPart>().InputDirection);
        //Debug.Log("old" + segID + ": " + mSegs[index].GetComponent<SegmentPart>().Input + " , " + mSegs[index].GetComponent<SegmentPart>().Output);
        //Debug.Log("new" + segID + ": " + newSegment.GetComponent<SegmentPart>().Input + " , " + newSegment.GetComponent<SegmentPart>().Output);
    }

    private bool CheckCollision(List<GameObject> machine, Vector2 offset)
    {
        foreach (GameObject seg in machine)
        {
            //check for any collision
            if (!seg.GetComponent<SegmentLogic>().CheckSegmentOverlap(offset, "", false, false, 0))
            {
                //Debug.Log("col" + seg.name);
                return false;
            }
        }
        return true;
    }

    private bool CheckCollisionMirrored(List<GameObject> machine, Vector2 offset)
    {
        Vector2 newOffset = offset;
        Vector2 prevOutput = Vector2.zero;

        for (int i = 0; i < machine.Count; i++)
        {
            if (i > 0)
            {
                //calculate new offset: output - input where output is output of last iteration
                newOffset = prevOutput - machine[i].GetComponent<SegmentPart>().Input;
            }
            //calculate mirrored output location, newOutput = input + offset + (output - input) * (-1,1)
            prevOutput = machine[i].GetComponent<SegmentPart>().Input + newOffset + (machine[i].GetComponent<SegmentPart>().Output - machine[i].GetComponent<SegmentPart>().Input) * new Vector2(-1, 1);

            //check for any collision
            if (!machine[i].GetComponent<SegmentLogic>().CheckSegmentOverlap(newOffset, "", false, true, 0))
            {
                return false;
            }
        }
        return true;
    }

    private void MoveMachine(List<GameObject> machine, Vector2 offset)
    {
        foreach (GameObject seg in machine)
        {
            if (seg == newSegment)
            {
                continue;
            }
            seg.GetComponent<SegmentPart>().MoveSegmentBy(offset);
        }
    }

    private void MoveMachineMirrored(List<GameObject> machine, Vector2 offset)
    {
        Vector2 mirrOffset = Vector2.zero;

        for (int i = 0; i < machine.Count; i++)
        {
            if (i == 0)
            {
                if (machine[i] == newSegment)
                {
                    continue;
                }
                mirrOffset = offset;
            }
            else
            {
                mirrOffset = machine[i - 1].GetComponent<SegmentPart>().Output - machine[i].GetComponent<SegmentPart>().Input;
            }
            //Debug.Log(mirrOffset);
            machine[i].GetComponent<SegmentPart>().MoveSegmentBy(mirrOffset);
            machine[i].GetComponent<SegmentPart>().MirrorSegment();
        }
    }

    private void SwitchGameObjectsState(List<GameObject> go, bool mode)
    {
        foreach (GameObject g in go)
        {
            g.SetActive(mode);
        }
    }
}
