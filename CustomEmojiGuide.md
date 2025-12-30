# Custom Emoji Guide for SmartExpense Notifications

## 🎯 How to Add Custom Emojis to Notifications

### 1. **Built-in Enemy Emojis Already Available:**
- 😈 (Devil) - For critical budget alerts (100%+ exceeded)
- 👹 (Ogre) - For urgent budget alerts (90-99% used)
- 👺 (Tengu) - For severe warnings
- 💀 (Skull) - For danger alerts
- 🤖 (Robot) - For system notifications
- 👾 (Alien Monster) - For fun notifications
- 🎃 (Pumpkin) - For seasonal alerts
- 👻 (Ghost) - For mysterious notifications
- 💩 (Poop) - For bad spending habits
- 🤡 (Clown) - For silly overspending

### 2. **How to Use Custom Emojis:**

#### **Method 1: In Controllers (Recommended)**
```csharp
// For transaction notifications
await _notificationService.SendCustomNotificationAsync(userId, 
    "You spent too much money!", "💀");

// For budget alerts
await _notificationService.SendBudgetAlertWithCustomIconAsync(userId, 
    category, percentage, "😈");
```

#### **Method 2: Direct Message with Emoji**
```csharp
// Just add emoji to the message
string message = "😈 Budget exceeded! You spent $" + amount;
await _notificationService.SendNotificationToUserAsync(userId, message);
```

### 3. **Examples of Usage:**

#### **Budget Alerts:**
- **75% used**: ⚠️ Budget Warning
- **90% used**: 👹 URGENT: Almost at budget limit!
- **100%+ exceeded**: 😈 CRITICAL: Budget destroyed!

#### **Transaction Types:**
- **Food overspending**: 🍕💸 You spent $50 on pizza!
- **Shopping addiction**: 🛍️😈 Shopping spree detected!
- **Emergency expense**: 🚑💸 Emergency medical expense

#### **Fun Notifications:**
- **First transaction**: 🎉 Congratulations on your first transaction!
- **Saving milestone**: 🏆 You saved $100 this month!
- **Bad spending day**: 💩 Today was expensive!

### 4. **How to Add Your Own Emojis:**

#### **Step 1:** Find your emoji (copy from sites like https://emojipedia.org)
#### **Step 2:** Add it to the notification service
```csharp
// Add your custom emoji to the message
await _notificationService.SendCustomNotificationAsync(userId, 
    "Custom message here", "🦸"); // Your custom emoji
```

#### **Step 3:** The system will automatically detect and display it!

### 5. **Popular Enemy/Warning Emojis:**
- 😈 Devil (Critical overspending)
- 👹 Ogre (Urgent budget warning)
- 👺 Tengu (Severe warning)
- 💀 Skull (Danger zone)
- 🤖 Robot (Automated alerts)
- 👾 Alien Monster (Unusual spending)
- 🎃 Pumpkin (Seasonal overspending)
- 👻 Ghost (Mysterious expenses)
- 🦹 Superhero (Saving money!)
- 🦴 Skeleton (Budget is dead)

### 6. **Current Implementation:**
Your system already uses:
- 😈 for budgets exceeded 100%
- 👹 for budgets at 90-99%
- 💰 for income
- 💸 for expenses
- 📧 for contact forms

### 7. **Test It:**
1. Add a transaction that exceeds your budget
2. You should see 😈 or 👹 in the notification!
3. The emoji will appear as a large icon in the notification

Enjoy your custom emoji notifications! 🎉
