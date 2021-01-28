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

    private System.Random random = new System.Random();

    private void Awake()
    {
        sL = gameObject.AddComponent<SegmentSelectionLogic>();
    }

    public IEnumerator MutateMachines(List<GameObject> population)
    {
        foreach(GameObject machine in population)
        {
            //if(machine.GetComponent<Machine>().Fitness == 0)
            //{
            //    AssignMutation(machine);
            //}
        }

        AssignMutation(population[0]);

        yield return null;
    }

    private void AssignMutation (GameObject machine)
    {
        mRef = machine;

        if(random.NextDouble() < SettingsReader.Instance.EASettings.MutationRate)
        {
            int method = Random.Range(0 , 4);
            switch (method)
            {
                case 0:
                    Mutate(machine, DeleteSegment, TryDeleteSegment);
                    break;
                case 1:
                    Mutate(machine, AddSegment, TryAddSegment);
                    break;
                case 2:
                    Mutate(machine, ReplaceSegment, TryChangeSegment);
                    break;
                default:
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

        Debug.Log("mutating: " + machine.name + " at: " + index + " -> " + mutationDelegate.Method.Name);

        //if last segment
        if (index == mSegs.Count - 1)
        {
            //check if new segment can fit at end
            //deletion always works
            if (tryMutationDelegate(mSegs, index))
            {
                //if yes add/replace in list or delete
                mutationDelegate(mSegs, index);
            } else
            {
                //Debug.Log("can not fit mutation at last");
                Destroy(newSegment);
                SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
            }
        } else
        {
            //disable latter half of machine
            SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), false);
            Vector2 offset;
            //check if new segment fits
            if (tryMutationDelegate(mSegs, index))
            {
                //if yes calculate offset via output of new segment, if deleting set output to input
                offset = newSegment.GetComponent<SegmentPart>().Output - mSegs[index + 1].GetComponent<SegmentPart>().Input;
                //Debug.Log(offset);
            } else
            {
                //if not reactivate machine, destroy new segment, and go back to original
                //Debug.Log("could not fit new mutation alone inbetween");
                Destroy(newSegment);
                SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
                return;
            }
            //check if rest of machine still fits after
            if (CheckCollision(mSegs.GetRange(index + 1, mSegs.Count - index - 1), offset))
            {
                //if yes add/replace new segment, move rest and reactivate
                mutationDelegate(mSegs, index);
                MoveMachine(mSegs.GetRange(index, mSegs.Count - index), offset);
                SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
                //Debug.Log("mutation successfull");
            } else
            {
                //if not reactivate machine, destroy new segment, and go back to original
                //Debug.Log("could not fit inbetween, going back to original");
                if (mSegs.Contains(newSegment))
                {
                    mSegs.Remove(newSegment);
                }
                Destroy(newSegment);
                SwitchGameObjectsState(mSegs.GetRange(index, mSegs.Count - index), true);
            }
        }
    }

    private bool TryDeleteSegment(List<GameObject> mSegs, int index)
    {
        newSegment = Instantiate(mSegs[index]);
        newSegment.GetComponent<SegmentPart>().Output = mSegs[index].GetComponent<SegmentPart>().Input;
        return true;
    }

    private bool TryAddSegment(List<GameObject> mSegs, int index)
    {
        //generate random segment ID, only try domino or balltrack because of mill bug
        int segID = UnityEngine.Random.Range(0, sL.NumPossibleSegments - 2);
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
        int segID = UnityEngine.Random.Range(0, sL.NumPossibleSegments);
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
    }

    private void AddSegment(List<GameObject> mSegs, int index)
    {
        //parent new segment and set sibling index to replaced
        newSegment.transform.parent = mRef.transform;
        newSegment.transform.SetSiblingIndex(mSegs[index+1].transform.GetSiblingIndex());
    }

    private void ReplaceSegment(List<GameObject> mSegs, int index)
    {
        //parent new segment and set sibling index to replaced
        newSegment.transform.parent = mRef.transform;
        newSegment.transform.SetSiblingIndex(mSegs[index].transform.GetSiblingIndex());
        Destroy(mSegs[index]);
        mSegs.RemoveAt(index);
        mSegs.Insert(index, newSegment);
    }

    private void GenerateNewSegment(List<GameObject> mSegs, int index, int segID)
    {
        newSegment = new GameObject("SegmentNew");
        sL.AssignSegment(newSegment, segID);
        sL.SetSegmentInOutput(newSegment, mSegs[index].GetComponent<SegmentPart>().Input, mSegs[index].GetComponent<SegmentPart>().GetDirection());
        //Debug.Log("old" + segID + ": " + mSegs[index].GetComponent<SegmentPart>().Input + " , " + mSegs[index].GetComponent<SegmentPart>().Output);
        //Debug.Log("new" + segID + ": " + newSegment.GetComponent<SegmentPart>().Input + " , " + newSegment.GetComponent<SegmentPart>().Output);
    }

    private bool CheckCollision(List<GameObject> machine, Vector2 offset)
    {
        foreach (GameObject seg in machine)
        {
            //check for any collision but restriction area
            if (!seg.GetComponent<SegmentLogic>().CheckEnoughRoom(seg.GetComponent<SegmentPart>().Input, seg.GetComponent<SegmentPart>().Output, offset, "", false))
            {
                //Debug.Log("col" + seg.name);
                return false;
            }
        }
        return true;
    }

    private void MoveMachine(List<GameObject> machine, Vector2 offset)
    {
        foreach(GameObject seg in machine)
        {
            if (seg == newSegment)
            {
                continue;
            }
            seg.GetComponent<SegmentPart>().MoveSegment(offset);
        }
    }

    private void SwitchGameObjectsState(List<GameObject> go, bool mode)
    {
        foreach(GameObject g in go)
        {
            g.SetActive(mode);
        }
    }
}
