use crate::CommandArgs;

pub struct CommandInfo {
    pub path: Vec<String>,
    pub description: String,
    pub key: String,
    pub handler: fn(&CommandArgs),
}

impl CommandInfo {
    pub fn new(
        path: Vec<String>,
        description: impl Into<String>,
        handler: fn(&CommandArgs),
    ) -> Self {
        let key = path.join(".");
        CommandInfo {
            path,
            description: description.into(),
            key,
            handler,
        }
    }
}
