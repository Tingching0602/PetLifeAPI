using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace petlife.Models;

/// <summary>
/// Basic profile for an authenticated Firebase user. Designed to be stored in Firestore.
/// </summary>
[FirestoreData]
public class UserProfile
{
 // Use Firebase UID as the document id when saving to Firestore
 [FirestoreDocumentId]
 public string Uid { get; set; } = default!;

 [FirestoreProperty]
 public string? Email { get; set; }

 [FirestoreProperty]
 public bool EmailVerified { get; set; }

 [FirestoreProperty]
 public string? DisplayName { get; set; }

 [FirestoreProperty]
 public string? PhotoUrl { get; set; }

 // e.g., password, google.com, facebook.com, apple.com
 [FirestoreProperty]
 public string? Provider { get; set; }

 [FirestoreProperty]
 public string? PhoneNumber { get; set; }

 // App tracking fields
 [FirestoreProperty]
 public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

 [FirestoreProperty]
 public DateTimeOffset? LastSignInAt { get; set; }

 [FirestoreProperty]
 public string? LastIp { get; set; }

 // Optional app roles/claims you want to persist
 [FirestoreProperty]
 public IList<string>? Roles { get; set; }
}