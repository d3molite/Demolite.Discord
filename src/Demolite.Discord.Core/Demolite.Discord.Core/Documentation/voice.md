# Join To Create Voice Channels

## Configuration

Set per guild in `ToolsConfig`:

| Setting | Purpose |
|---|---|
| `VoiceChannelCreateId` | Trigger channel. Created channels are placed in its category. |
| `VoiceChannelRenameAllowed` | Enables the rename button. Null counts as disabled. |
| `VoiceChannelDeleteDelaySeconds` | The delay in seconds after which an empty channel should be deleted. Defaults to 60 |

`GuildConfig.LoggingCulture` selects the language of the rename texts in `MessageResources`.

## Flow

### Create
1. A user joins the trigger channel.
2. A channel named `{number} | {username}` is created in the trigger's category. The number is the lowest free one in the guild. A lock prevents duplicate numbers on simultaneous joins.
3. The channel is added to `VoiceChannelCache`.
4. If renaming is allowed, the rename message with its button is posted in the channel.
5. A deletion is scheduled as a safety net, then the user is moved into the channel.

### Delete
1. A user leaves a tracked channel and no users remain, so a one-minute deletion is scheduled.
2. A user joining a tracked channel cancels the pending deletion.
3. When the delay expires, the channel is deleted only if it is still empty, then removed from the cache.

### Restart recovery
1. On guild create, `VoiceStateCache` is filled from the guild's current voice states.
2. Voice channels in the trigger's category whose name starts with a number prefix are added to `VoiceChannelCache`.
3. Recovered channels that are empty get a scheduled deletion.
### Rename
1. A user clicks the button on the rename message.
2. The access check requires that renaming is allowed, the channel is tracked, and the user is in that channel. Otherwise an ephemeral notice is shown.
3. A modal opens, prefilled with the current name without the number.
4. On submit the access check runs again, then the channel is renamed to the entered name with its original number. The number comes from `VoiceChannelCache` and cannot be changed by the user.