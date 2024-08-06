using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiLayerImageController : MonoBehaviour
{
    private static Image subImagePrefab;
    private static Image SubImagePrefab {
        get {
            if (subImagePrefab == null)
                subImagePrefab = Resources.Load<Image>("Prefabs/UI/SubImage");
            return subImagePrefab;
        }
    }

    [SerializeField] private Transform iconImagesParent;

    private List<Image> iconImages = new List<Image>();

    public List<Sprite> Sprites { 
        set {
            if (value == null)
                return;

            while (iconImages.Count < value.Count) {
                iconImages.Add(Instantiate(SubImagePrefab, iconImagesParent));
            }
            for (int i = 0; i < iconImages.Count; ++i) {
                if (i < value.Count) {
                    iconImages[i].sprite = value[i];
                    iconImages[i].gameObject.SetActive(true);
                }
                else {
                    iconImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

}
