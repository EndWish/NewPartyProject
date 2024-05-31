using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum Tag
{
    None = 0,

    물리피해유형시작 = 01_01_00,
        충격, 관통, 베기, 물리피해유형끝 = 01_01_99,

    속성피해유형시작 = 01_02_00,
        물, 불, 바람, 전기, 독, 속성피해유형끝 = 01_02_99,

    상태이상유형시작 = 02_01_00,
        출혈, 화상, 젖음, 감전, 중독, 상태이상유형끝 = 02_01_99,

    공격형태시작 = 03_01_00,
        기본공격, 스킬공격, 근거리, 원거리, 광역기, 공격형태끝 = 03_01_99,

    종족시작 = 04_01_00,
        인간, 견족, 묘족, 언데드, 슬라임, 새, 종족끝 = 04_01_99,

    특징시작 = 04_02_00,
        비행, 잠수, 특징끝 = 04_02_00,
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
