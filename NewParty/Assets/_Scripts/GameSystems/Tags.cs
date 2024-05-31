using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum Tag
{
    None = 0,

    ���������������� = 01_01_00,
        ���, ����, ����, �������������� = 01_01_99,

    �Ӽ������������� = 01_02_00,
        ��, ��, �ٶ�, ����, ��, �Ӽ����������� = 01_02_99,

    �����̻��������� = 02_01_00,
        ����, ȭ��, ����, ����, �ߵ�, �����̻������� = 02_01_99,

    �������½��� = 03_01_00,
        �⺻����, ��ų����, �ٰŸ�, ���Ÿ�, ������, �������³� = 03_01_99,

    �������� = 04_01_00,
        �ΰ�, ����, ����, �𵥵�, ������, ��, ������ = 04_01_99,

    Ư¡���� = 04_02_00,
        ����, ���, Ư¡�� = 04_02_00,
}

public class Tags
{
    private SortedDictionary<Tag, int> tags = new SortedDictionary<Tag, int>();

    public Tags() { }
    public Tags(params Tag[] tags) {
        AddTag(tags);
    }

    public void AddTag(Tag tag) {
        if (tags.ContainsKey(tag))
            ++tags[tag];
        else
            tags.Add(tag, 1);
    }
    public void AddTag(params Tag[] tags) {
        foreach (Tag tag in tags)
            AddTag(tag);
    }

    public void SubTag(Tag tag) {
        --tags[tag];
        if (tags[tag] <= 0)
           tags.Remove(tag);
    }
    public void SubTag(params Tag[] tags) {
        foreach (Tag tag in tags)
            SubTag(tag);
    }

    public string GetString() {
        StringBuilder sb = new StringBuilder();
        foreach(Tag tag in tags.Keys) {
            sb.Append("#").Append(tag.ToString()).Append(" ");
        }
        return sb.ToString();
    }

    public bool Contains(Tag tag) {
        return tags.ContainsKey(tag);
    }
    public bool ContainsAll(Tags otherTags) {
        IDictionaryEnumerator enumerator = this.tags.GetEnumerator();
        bool hasNext = enumerator.MoveNext();

        foreach (Tag tag in otherTags.tags.Keys) {
            while(hasNext && (Tag)enumerator.Key < tag) {
                hasNext = enumerator.MoveNext();
            }

            if (!hasNext)
                return false;

            if ((Tag)enumerator.Key != tag)
                return false;
        }
        return true;
    }
    public bool ContainsAtLeastOne(Tags otherTags) {
        IDictionaryEnumerator enumerator = this.tags.GetEnumerator();
        bool hasNext = enumerator.MoveNext();

        foreach (Tag tag in otherTags.tags.Keys) {
            while (hasNext && (Tag)enumerator.Key < tag) {
                hasNext = enumerator.MoveNext();
            }

            if (!hasNext)
                return false;

            if ((Tag)enumerator.Key == tag)
                return true;
        }
        return false;
    }
    
}
