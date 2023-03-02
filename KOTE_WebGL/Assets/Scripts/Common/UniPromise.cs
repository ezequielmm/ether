using Cysharp.Threading.Tasks;
using System;

public class UniPromise<T>
{
    public bool Completed { get; private set; }
    public Guid Id { get; private set; }
    public T Data { get; private set; }
    public DateTime CreationTime { get; private set; }

    public void FulfillRequest(T data)
    {
        Data = data;
        Completed = true;
    }

    public UniPromise() 
    {
        Data = default(T);
        Completed = false;
        CreationTime = DateTime.Now;
        Id = Guid.NewGuid();
    }

    public async UniTask WaitForFufillment() 
    {
        await UniTask.WaitUntil(() => this.Completed);
    }
}
