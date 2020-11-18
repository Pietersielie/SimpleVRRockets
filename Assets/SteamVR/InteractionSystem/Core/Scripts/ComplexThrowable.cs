//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Throwable that uses physics joints to attach instead of just
//			parenting
//
//=============================================================================

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Interactable ) )]
	public class ComplexThrowable : MonoBehaviour
	{
		public enum AttachMode
		{
			FixedJoint,
			Force,
		}

		public GameObject rocketPrefab;

		public float attachForce = 800.0f;
		public float attachForceDamper = 25.0f;

		public AttachMode attachMode = AttachMode.FixedJoint;

		[EnumFlags]
		public Hand.AttachmentFlags attachmentFlags = 0;

		private List<Hand> holdingHands = new List<Hand>();
		private List<Rigidbody> holdingBodies = new List<Rigidbody>();
		private List<Vector3> holdingPoints = new List<Vector3>();

		private List<Rigidbody> rigidBodies = new List<Rigidbody>();

		private List<GameObject> attachmentPoints = new List<GameObject>();
		private List<GameObject> attachmentPointsHere = new List<GameObject>();
		private List<GameObject> attachmentPointsOther = new List<GameObject>();

		private List<GameObject> sideAttachmentPoints = new List<GameObject>();
		private List<GameObject> sideAttachmentPointsHere = new List<GameObject>();
		private List<GameObject> sideAttachmentPointsOther = new List<GameObject>();

		private GameObject connectedDrill = null;

		//-------------------------------------------------
		void Awake()
		{
			GetComponentsInChildren<Rigidbody>( rigidBodies );
		}


		//-------------------------------------------------
		void Update()
		{
			
		}

		private bool ObjectInRigidbodies(GameObject gameObject) {
			for ( int i = 0; i < rigidBodies.Count; i++)
            {
				if (rigidBodies[i].gameObject == gameObject) {
					return true;

				}
            }
			return false;
		}

		private GameObject ObjectRocket(GameObject gameObject)
		{
			if (gameObject.transform.parent) 
			{
				if (gameObject.transform.parent.gameObject.tag == "Rocket" || gameObject.transform.parent.gameObject.tag == "LaunchableRocket")
				{
					return gameObject.transform.parent.gameObject;
				}
			}
			return null;
		}

		private GameObject OddOneOut(GameObject[] before, GameObject[] after)
		{
            foreach (GameObject item in after)
            {
				if (!Array.Exists(before, element => element == item))
				{
					return item;
				}
            }
			return null;
		}

		//-------------------------------------------------
		private void OnHandHoverBegin( Hand hand )
		{
			if ( holdingHands.IndexOf( hand ) == -1 )
			{
				if ( hand.isActive )
				{
					hand.TriggerHapticPulse( 800 );
				}
			}
		}


		//-------------------------------------------------
		private void OnHandHoverEnd( Hand hand )
		{
			if ( holdingHands.IndexOf( hand ) == -1 )
			{
				if (hand.isActive)
				{
					hand.TriggerHapticPulse( 500 );
				}
			}
		}


		//-------------------------------------------------
		private void HandHoverUpdate( Hand hand )
		{
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
			{
				PhysicsAttach( hand, startingGrabType );
			}
		}


		//-------------------------------------------------
		private void PhysicsAttach( Hand hand, GrabTypes startingGrabType )
		{
			PhysicsDetach( hand );

			Rigidbody holdingBody = null;
			Vector3 holdingPoint = Vector3.zero;

			// The hand should grab onto the nearest rigid body
			float closestDistance = float.MaxValue;
			for ( int i = 0; i < rigidBodies.Count; i++ )
			{
				float distance = Vector3.Distance( rigidBodies[i].worldCenterOfMass, hand.transform.position );
				if ( distance < closestDistance )
				{
					holdingBody = rigidBodies[i];
					closestDistance = distance;
				}
			}

			// Couldn't grab onto a body
			if ( holdingBody == null )
				return;

			// Create a fixed joint from the hand to the holding body
			if ( attachMode == AttachMode.FixedJoint )
			{
				Rigidbody handRigidbody = Util.FindOrAddComponent<Rigidbody>( hand.gameObject );
				handRigidbody.isKinematic = true;

				FixedJoint handJoint = hand.gameObject.AddComponent<FixedJoint>();
				handJoint.connectedBody = holdingBody;
			}

			// Don't let the hand interact with other things while it's holding us
			hand.HoverLock( null );

			// Affix this point
			Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
			offset = Mathf.Min( offset.magnitude, 1.0f ) * offset.normalized;
			holdingPoint = holdingBody.transform.InverseTransformPoint( holdingBody.worldCenterOfMass + offset );

			hand.AttachObject( this.gameObject, startingGrabType, attachmentFlags );

			// Update holding list
			holdingHands.Add( hand );
			holdingBodies.Add( holdingBody );
			holdingPoints.Add( holdingPoint );
		}


		//-------------------------------------------------
		private bool PhysicsDetach( Hand hand )
		{
			int i = holdingHands.IndexOf( hand );

			if ( i != -1 )
			{
				// Detach this object from the hand
				holdingHands[i].DetachObject( this.gameObject, false );

				// Allow the hand to do other things
				holdingHands[i].HoverUnlock( null );

				// Delete any existing joints from the hand
				if ( attachMode == AttachMode.FixedJoint )
				{
					Destroy( holdingHands[i].GetComponent<FixedJoint>() );
				}

				Util.FastRemove( holdingHands, i );
				Util.FastRemove( holdingBodies, i );
				Util.FastRemove( holdingPoints, i );

				return true;
			}

			return false;
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.gameObject.tag == "Drill")
			{
				connectedDrill = other.gameObject;
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.gameObject.tag == "Drill")
			{
				connectedDrill = null;
			}
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			if ( attachMode == AttachMode.Force )
			{
				for ( int i = 0; i < holdingHands.Count; i++ )
				{
					Vector3 targetPoint = holdingBodies[i].transform.TransformPoint( holdingPoints[i] );
					Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

					holdingBodies[i].AddForceAtPosition( attachForce * vdisplacement, targetPoint, ForceMode.Acceleration );
					holdingBodies[i].AddForceAtPosition( -attachForceDamper * holdingBodies[i].GetPointVelocity( targetPoint ), targetPoint, ForceMode.Acceleration );
				}
			}
			for (int i = 0; i < holdingHands.Count; i++)
			{
				if (holdingHands.IndexOf(holdingHands[i].otherHand) == -1 && holdingHands[i].gameObject.tag=="RightHand") // other hand not holding this
				{
					Hand otherHand = holdingHands[i].otherHand;
					if (otherHand.currentAttachedObject)                      // and holding a thing
					{
						var attpArray = GameObject.FindGameObjectsWithTag("AttachmentPoint");
						var sideattpArray = GameObject.FindGameObjectsWithTag("SideAttachmentPoint");
						foreach (GameObject item in attpArray)
						{
							attachmentPoints.Add(item);
						}
						foreach (GameObject item in sideattpArray)
						{
							sideAttachmentPoints.Add(item);
						}
						for (int j = 0; j < attachmentPoints.Count; j++)
						{
							GameObject attachmentPoint = attachmentPoints[j];
							if (ObjectInRigidbodies(attachmentPoint.transform.parent.parent.gameObject))
							{
								attachmentPointsHere.Add(attachmentPoint);
							}
							else if (attachmentPoint.transform.parent.parent.gameObject == otherHand.currentAttachedObject)
							{
								attachmentPointsOther.Add(attachmentPoint);
							}
						}
						for (int j = 0; j < sideAttachmentPoints.Count; j++)
						{
							GameObject sideAttachmentPoint = sideAttachmentPoints[j];
							if (ObjectInRigidbodies(sideAttachmentPoint.transform.parent.parent.gameObject))
							{
								sideAttachmentPointsHere.Add(sideAttachmentPoint);
							}
							else if (sideAttachmentPoint.transform.parent.parent.gameObject == otherHand.currentAttachedObject)
							{
								sideAttachmentPointsOther.Add(sideAttachmentPoint);
							}
						}
						float closestDistance = float.MaxValue;
						GameObject closestHere = null;
						GameObject closestOther = null;
						bool isSide = false;
						for (int j = 0; j < attachmentPointsHere.Count; j++)
						{
							for (int k = 0; k < attachmentPointsOther.Count; k++)
							{
								float distance = Vector3.Distance(attachmentPointsHere[j].transform.position, attachmentPointsOther[k].transform.position);
								if (distance < closestDistance)
								{
									closestDistance = distance;
									closestHere = attachmentPointsHere[j];
									closestOther = attachmentPointsOther[k];
								}
							}
						}
						for (int j = 0; j < sideAttachmentPointsHere.Count; j++)
						{
							for (int k = 0; k < sideAttachmentPointsOther.Count; k++)
							{
								float distance = Vector3.Distance(sideAttachmentPointsHere[j].transform.position, sideAttachmentPointsOther[k].transform.position);
								if (distance < closestDistance)
								{
									closestDistance = distance;
									closestHere = sideAttachmentPointsHere[j];
									closestOther = sideAttachmentPointsOther[k];
									isSide = true;
								}
							}
						}
						if (closestDistance < 0.01 && (!isSide || connectedDrill))
						{
							// alignment step?
							GameObject otherPart = closestOther.transform.parent.parent.gameObject;
							FixedJoint APJoint = closestHere.transform.parent.parent.gameObject.AddComponent<FixedJoint>();
							APJoint.connectedBody = otherPart.GetComponent<Rigidbody>();
							otherHand.SendMessage("DetachObject", otherHand.currentAttachedObject);

							GameObject thisRocket = ObjectRocket(this.gameObject);
							GameObject thatRocket = ObjectRocket(otherPart);
							if (thisRocket == null && thatRocket == null)
							{
								var preRockets = GameObject.FindGameObjectsWithTag("Rocket");
								var preLaunchableRockets = GameObject.FindGameObjectsWithTag("LaunchableRocket");
								var beforeRockets = new GameObject[preRockets.Length + preLaunchableRockets.Length];
								preRockets.CopyTo(beforeRockets, 0);
								preLaunchableRockets.CopyTo(beforeRockets, preRockets.Length);
								Instantiate(rocketPrefab, closestHere.transform.position, Quaternion.identity);
								var postRockets = GameObject.FindGameObjectsWithTag("Rocket");
								var postLaunchableRockets = GameObject.FindGameObjectsWithTag("LaunchableRocket");
								var afterRockets = new GameObject[postRockets.Length + postLaunchableRockets.Length];
								postRockets.CopyTo(afterRockets, 0);
								postLaunchableRockets.CopyTo(afterRockets, postRockets.Length);
								GameObject newRocket = OddOneOut(beforeRockets, afterRockets);
								newRocket.SendMessage("AddPart", this.gameObject);
								newRocket.SendMessage("AddPart", otherPart);
							}
							else if (thisRocket == null)
							{
								thatRocket.SendMessage("AddPart", this.gameObject);
							}
							else if (thatRocket == null)
							{
								thisRocket.SendMessage("AddPart", otherPart);
							}
							else
							{
								thisRocket.SendMessage("AddRocket", thatRocket);
							}
						}
						attachmentPoints.Clear();
						attachmentPointsHere.Clear();
						attachmentPointsOther.Clear();
					}
				}

				if (holdingHands[i].IsGrabEnding(this.gameObject))
				{
					PhysicsDetach(holdingHands[i]);
				}
			}
		}
	}
}
