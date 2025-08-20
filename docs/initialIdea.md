# Initial Idea Document

## Introduction

After looking into gaps of educational content in computer science, I found that there is a lack of educational games for SQL that don't utilise more abstract analogies for SQL. This is likely due to the fact that SQL can often be seen as alredy intuitive enough and simple, making educators assume that there is no need to create further tools than the occasional interactive session with a placeholder database. However, what I have noticed recently with a sizeable portion of my peers is that they end up "kind of" understanding individual commands, the gist of database structure, and in some cases, relational algebra, but not the action of using and compounding SQL statements themselves into abritrary / compounded queries. This problem only becomes worse due to the fact that many students are almost expected to be already familiar with the flow of using SQL beforehand due to exposure in secondary school and sixth form / college.

This is the case for a majority of CS students, but in the case of people entering the field at an undergraduate level, they get left behind for the sake of brevity of module content. This is, in my opinion, is a grave oversight and many are left to simply figure it out on their own or to get taught in an impromptu way by more experienced peers, with varying results in both cases. Many don't even end up inherently understanding what it is they are doing at an abstract level. I believe that preparing such students to *think* in the same type of queries as SQL without even realising it in a less stressful and simpler environment can potentially prepare them better for this learning experience. This approach requires a deviation *away* from SQL, as the immediate use of SQL only serves to confuse students who don't understand it from the beginning and are constrained by time, making trial and error unviable. Gradually introducing SQL into a set of actions they are first familiarised with is, in my opinion, a much better approach to introducing SQL usage to people that don't begin understanding it.

## Core Ideas

There are three core ideas of what this type of game should strive for in my vision:

- **Analogy**: Provide players with an analogy of what SQL queries actually do, one which abstracts away the ideas of tables, databases and queries, into something simpler to grasp. The elimination of terminology dumping lets players only focus on the action, not the information.
- **Visualisation**: Let players *see* what it is that they are doing, what kind of mistake they are making whenever their queries are incorrect, or how their query ends up returning the correct result. That is, to show the procedure of "filtering through" the records. This helps provide an intuition for what the commands "look like" when applied to a certain set of records.
- **Distraction through Context**: Distract the players from the fact they're actually learning to think in SQL by not using or mentioning it while teaching the logic of SQL. This can be later paired with a "bridging of the gap" where the layers of context are slowly peeled away to reveal the connection. A lot of educational games have this pitfall, where they market themselves as "maths games" or something else, which immediately lessens the players' interest in the process by making them think they are actually studying. Hacknet is an excellent example of this, where the gamification of its tasks makes the player forget they are being taught both how to use the command line and networking concepts simultaneously.

## Content

In the topic of SQL, there are multiple areas of potential learning. For the initial game, I will base the content of the levels on the Computer Science A level OCR specification, as it feels concrete without being too bloated. Students are expected to understand the following commands:

- `SELECT`
- `FROM`
- `WHERE`
- `ORDER BY`
- `LIKE`
- `AND`
- `OR`
- `DELETE`
- `INSERT`
- `DROP`
- `JOIN` (this only including `INNER JOIN`)
- The wildcards `*` and `%`

I believe these commands can be seen as the essential commands of SQL. Once these are understood fluently, the rest can easily fall into place, or be implemented in further developments of the game.

## Technology Used

To make this game, I plan to use the Unity engine because of my previous familiarity with it, and its ease of use for both 2D and 3D games. In particular, I'll use version 6.2, the most recent version at the time of creation. The version of SQL that the game will be based on is SQLite, due to its simple syntax and ease of use.
