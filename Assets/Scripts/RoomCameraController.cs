using System.Collections.Generic;
using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 3, -10);
    public float moveSmoothTime = 0.3f;
    public float transitionSmooth = 0.1f;
    public float playerInfluence = 2f;
    public float roomSwitchSpeed = 5f;

    private Transform targetFocusPoint;
    private Transform followTarget;
    private CameraRoomTrigger currentRoom;

    private Vector3 velocity = Vector3.zero;
    private float minX = float.NegativeInfinity;
    private float maxX = float.PositiveInfinity;

    private Stack<CameraRoomTrigger> roomHistory = new Stack<CameraRoomTrigger>();

    #region Room Switching

    public void MoveToRoom(CameraRoomTrigger room)
    {
        if (room == null || room.cameraFocusPoint == null) return;

        if (currentRoom != null && currentRoom != room)
            roomHistory.Push(currentRoom);

        currentRoom = room;
        targetFocusPoint = room.cameraFocusPoint;

        minX = room.minX;
        maxX = room.maxX;
    }

    public void MoveBackToPreviousRoom()
    {
        if (roomHistory.Count == 0) return;

        CameraRoomTrigger previousRoom = roomHistory.Pop();
        if (previousRoom == null || previousRoom.cameraFocusPoint == null) return;

        currentRoom = previousRoom;
        targetFocusPoint = previousRoom.cameraFocusPoint;
        minX = previousRoom.minX;
        maxX = previousRoom.maxX;
    }

    public void SetFollowTarget(Transform character)
    {
        followTarget = character;
    }

    #endregion

    private void LateUpdate()
    {
        if (targetFocusPoint == null) return;

        Vector3 targetPos = targetFocusPoint.position + offset;

        if (followTarget != null)
        {
            Vector3 delta = followTarget.position - targetFocusPoint.position;
            delta.y = 0;
            Vector3 parallax = Vector3.ClampMagnitude(delta, playerInfluence);
            targetPos += parallax;
        }

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, moveSmoothTime);
    }

    #region Helpers

    public CameraRoomTrigger GetCurrentRoom() => currentRoom;

    public void SetInstant(CameraRoomTrigger room)
    {
        if (room == null || room.cameraFocusPoint == null) return;

        currentRoom = room;
        targetFocusPoint = room.cameraFocusPoint;
        transform.position = targetFocusPoint.position + offset;
        minX = room.minX;
        maxX = room.maxX;
        velocity = Vector3.zero;
    }

    #endregion
}










