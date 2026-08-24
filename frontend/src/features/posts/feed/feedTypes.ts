export type FeedKind = 'anonymous' | 'global' | 'personal'
export type SelectableFeedKind = Exclude<FeedKind, 'anonymous'>
