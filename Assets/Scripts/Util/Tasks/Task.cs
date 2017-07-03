using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Task {

	public enum TaskStatus : byte {
		Detached,
		Pending,
		Working,
		Success,
		Fail,
		Aborted
	}

	public TaskStatus status { get; private set; }

	public bool IsDetached {
		get {
			return (status == TaskStatus.Detached);
		}
	}

	public bool IsAttached {
		get {
			return (status != TaskStatus.Detached);
		}
	}

	public bool IsPending {
		get {
			return (status == TaskStatus.Pending);
		}
	}

	public bool IsWorking {
		get {
			return (status == TaskStatus.Working);
		}
	}

	public bool IsSuccessful {
		get {
			return (status == TaskStatus.Success);
		}
	}

	public bool IsFailure {
		get {
			return (status == TaskStatus.Fail);
		}
	}

	public bool IsAborted {
		get {
			return (status == TaskStatus.Aborted);
		}
	}

	public bool IsFinished{
		get {
			return ((status == TaskStatus.Aborted) || (status == TaskStatus.Fail) || (status == TaskStatus.Success));
		}
	}

	public void Abort(){
		SetStatus (TaskStatus.Aborted);
	}

	internal void SetStatus(TaskStatus newStatus){
		if (status == newStatus) {
			return;
		}
			
		status = newStatus;

		switch (newStatus) {
		case TaskStatus.Working:
			Init ();
			break;
		
		case TaskStatus.Aborted:
			OnAbort ();
			CleanUp ();
				break;
		
		case TaskStatus.Fail:
			OnFail ();
			CleanUp ();
				break;
		
		case TaskStatus.Success:
			OnSuccess ();
			CleanUp ();
			break;

		case TaskStatus.Detached:
		case TaskStatus.Pending:
			break;
			
		default:
			throw new ArgumentOutOfRangeException(newStatus.ToString(), newStatus, null);
		}
	}

	protected virtual void Init(){}

	protected virtual void OnAbort() {}

	protected virtual void OnSuccess() {}

	protected virtual void OnFail() {}

	internal virtual void Update() {}

	protected virtual void CleanUp() {}

	public List<Task> nextTasks{ get; private set; }

	public void Then(params Task[] tasks)
    {
        nextTasks = new List<Task>();
        foreach (Task task in tasks)
        {
            Debug.Assert(!task.IsAttached);
            nextTasks.Add(task);
        }
	}

    public void Then(List<Task> tasks)
    {
        nextTasks = new List<Task>();
        foreach (Task task in tasks)
        {
            Debug.Assert(!task.IsAttached);
            nextTasks.Add(task);
        }
    }

    public void Then(TaskTree taskTree)
    {
        Debug.Assert(!taskTree.root.IsAttached);
        nextTasks = new List<Task>(){ taskTree.DistributedTree() };
    }
}

public class TaskTree
{
    public Task root;
    public List<TaskTree> children;

    public TaskTree(Task _root, params TaskTree[] _children)
    {
        root = _root;
        children = new List<TaskTree>();
        foreach (TaskTree child in _children) children.Add(child);
    }

    public Task DistributedTree()
    {
        List<Task> childrenDistributed = new List<Task>();
        if (children.Count > 0) foreach (TaskTree child in children) childrenDistributed.Add(child.DistributedTree());
        root.Then(childrenDistributed);
        return root;
    }
}


