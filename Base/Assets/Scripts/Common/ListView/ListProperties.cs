using UnityEngine;
using System.Collections;

/*
 * List에 필요한 속성값 클래스
 */

public class ListProperties
{
    public bool isVertical = true;  //세로로 스크롤되는 List인지 여부
    public int max_lineCount;       //한번에 보여질 item의 최대 갯수

    public float listWidth;   //list의 가로크기
    public float listHeight;  //list의 세로크기

    public float itemWidth;   //item의 가로크기
    public float itemHeight;  //item의 세로크기

    public float itemPosZ;
    public float itemTopPosition; //item disappear position

    public float distanceOffset = 0.0f; //top item init position margin. must be greater than 0

    //Only use GridListView
    public int max_gridCount = 1;   //line에 보여질 item의 최대 갯수
    public float offset_gridPosition;   //item좌표의 offset : vertical이면 x좌표, horizontal이면 y좌표 offset을 의미
}
