# AI-Powered Research Paper Hub
## Visual Programming Semester Project Specification

---

## 1. Project Overview

**Project Name:** AI-Powered Research Paper Hub

**Purpose:** A multi-user web application that helps researchers and students discover, analyze, and collaborate on academic research papers using AI-powered summaries and intelligent recommendations.

**Key Concept:** Combines features from both a smart literature review tool AND a research paper recommendation engine, allowing individual researchers to discover papers and collaborative teams to work together on analyzing and organizing research.

---

## 2. Project Scope & Requirements

### Part 1: Blazor Web Application (Main User System)

#### 2.1.1 Authentication & User Management
- **Google OAuth Login** - Users authenticate via Google
- **User Profiles** - Store research interests, expertise areas, preferred topics
- **Account Settings** - Manage preferences, notification settings

#### 2.1.2 Paper Discovery & Search
- **Advanced Search** - Search by:
  - Keywords/phrases
  - Topic/field
  - Author name
  - Year range
  - Relevance filters
- **Search Integration** - Use SerpAPI to query Google Scholar and academic databases
- **Paper Details** - Display:
  - Title, authors, publication year
  - Abstract
  - Citation count
  - Research field/tags
  - Link to full paper

#### 2.1.3 AI-Powered Summaries
- **Automatic Summarization** - OpenAI API summarizes each paper
- **Summary Display** - Show:
  - Key findings (3-5 bullet points)
  - Methodology overview
  - Significance/impact
  - Related concepts
- **Save Summaries** - Users can bookmark and organize summaries

#### 2.1.4 Collaborative Research Groups
- **Create Groups** - Users create research groups around topics
- **Join Groups** - Share group code/link for other researchers to join
- **Group Dashboard** - Display:
  - All papers discovered by group members
  - Shared annotations and notes
  - Group research focus
  - Member list

#### 2.1.5 Literature Review Features
- **Paper Collection** - Add papers to group collections
- **Tagging System** - Tag papers with custom tags (methodology, findings, theory, etc.)
- **Notes & Annotations** - Team members can:
  - Add notes to papers
  - Highlight key sections
  - Share insights with group
- **Organization Tools** - Categorize papers by:
  - Theme/topic
  - Contribution type
  - Research phase

#### 2.1.6 Smart Recommendations
- **User Interest Tracking** - Track what papers users view/save
- **OpenAI-Powered Recommendations** - AI suggests relevant papers based on:
  - User's research history
  - Group's focus areas
  - Paper similarity
- **Recommendation Reasons** - Explain why a paper is recommended
- **Discovery Feed** - Personalized feed of recommended papers

#### 2.1.7 Group Analysis & Insights
- **Research Gap Analysis** - Identify understudied areas in group's research
- **Citation Network** - Show connections between papers (which papers cite which)
- **Trend Analysis** - What topics are emerging in the field
- **Summary Reports** - Generate group reports on research findings

#### 2.1.8 UI Features
- **Dashboard** - Overview of recent papers, recommendations, group activity
- **Responsive Design** - Works on desktop and tablet (Bootstrap/Tailwind)
- **Intuitive Navigation** - Clear menus, search bar, filters

---

### Part 2: Windows Forms Admin Dashboard

#### 2.2.1 User Monitoring
- **View all registered users**
- **User activity logs** - what searches, papers viewed, summaries generated
- **Search usage statistics**

#### 2.2.2 Group Management
- **View all research groups**
- **Group activity** - how many papers added, member engagement
- **Member statistics per group**
- **Most active groups**

#### 2.2.3 Paper Analytics
- **Most searched papers**
- **Most bookmarked papers**
- **Popular research fields/topics**
- **Trending topics over time**

#### 2.2.4 API Usage Monitoring
- **Track API calls** to:
  - SerpAPI (paper searches)
  - OpenAI (summaries generated)
  - OpenAlex (academic data retrieved)
- **Monitor costs** - Track API usage costs
- **Performance metrics** - Response times, success rates

#### 2.2.5 System Insights
- **Dashboard overview** - Key metrics at a glance
- **Reports** - Generate usage reports
- **Recommendations** - System health and optimization suggestions

#### 2.2.6 Data Management
- **Offline access** - Sync frequently accessed data locally (SQLite)
- **Data refresh** - Pull latest data from Web API
- **Export capabilities** - Export reports as CSV/PDF

---

### Part 3: Local Data Synchronization

#### 2.3.1 Local Database (SQLite)
- **Sync data** from main SQL Server for offline admin access
- **Cache** frequently accessed reports and analytics
- **Store** admin snapshots for historical comparison

#### 2.3.2 Offline Functionality
- **View reports** without internet connection
- **Generate reports** from cached data
- **View user/group statistics** from last sync

#### 2.3.3 Synchronization Logic
- **Manual sync** - Admin triggers data refresh from API
- **Auto-sync** - Periodic background sync (optional)
- **Conflict resolution** - Handle discrepancies between local and server

---

## 3. Technology Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Frontend | Blazor Server | User-facing web application |
| UI Styling | Bootstrap or Tailwind CSS | Responsive design |
| Backend | ASP.NET Core Web API | REST API for data & logic |
| Main Database | SQL Server | Primary data storage |
| Admin Desktop | Windows Forms | Admin monitoring dashboard |
| Local Admin DB | SQLite | Offline admin data cache |
| AI Integration | OpenAI API | Paper summaries, recommendations |
| Paper Search | SerpAPI | Search Google Scholar |
| Academic Data | OpenAlex API | Fetch research metadata |
| Authentication | Google OAuth 2.0 | User login |

---

## 4. Database Schema Overview

### Key Tables:

**Users Table**
- user_id (PK)
- email
- name
- google_id
- research_interests
- created_at

**ResearchGroups Table**
- group_id (PK)
- group_name
- description
- created_by (FK to Users)
- group_code (for sharing)
- created_at

**GroupMembers Table**
- group_id (FK)
- user_id (FK)
- joined_at

**Papers Table**
- paper_id (PK)
- title
- authors
- publication_year
- abstract
- url
- source (Google Scholar, etc.)
- citation_count
- fetched_at

**PaperSummaries Table**
- summary_id (PK)
- paper_id (FK)
- summary_text
- key_findings
- methodology
- generated_by_ai
- created_at

**GroupPapers Table**
- group_id (FK)
- paper_id (FK)
- added_by (FK to Users)
- added_at

**UserInterests Table**
- user_id (FK)
- topic
- interest_level
- created_at

**Notes & Annotations Table**
- note_id (PK)
- paper_id (FK)
- user_id (FK)
- group_id (FK)
- note_text
- created_at

**APIUsageLogs Table**
- log_id (PK)
- api_name (SerpAPI, OpenAI, etc.)
- endpoint
- timestamp
- success (bool)
- response_time_ms
- cost (if applicable)

---

## 5. API Endpoints (Backend)

### Authentication
- `POST /auth/google-login` - Authenticate user via Google
- `POST /auth/logout` - Logout user
- `GET /auth/user-profile` - Get current user's profile

### Paper Search
- `GET /papers/search?query={query}&field={field}&year_from={year}&year_to={year}` - Search papers via SerpAPI
- `GET /papers/{paper_id}` - Get paper details
- `GET /papers/{paper_id}/summary` - Get AI summary
- `POST /papers/{paper_id}/bookmark` - Bookmark a paper
- `GET /user/bookmarks` - Get user's saved papers

### Groups
- `POST /groups` - Create new group
- `GET /groups` - List all groups (user's groups)
- `GET /groups/{group_id}` - Get group details
- `PUT /groups/{group_id}` - Update group
- `POST /groups/{group_id}/members` - Add member to group
- `GET /groups/{group_id}/papers` - Get papers in group
- `POST /groups/{group_id}/papers/{paper_id}` - Add paper to group

### Recommendations
- `GET /recommendations` - Get personalized recommendations
- `GET /recommendations/reasons/{paper_id}` - Why this paper is recommended

### Admin Analytics (Secured endpoints)
- `GET /admin/users` - List all users
- `GET /admin/groups` - List all groups
- `GET /admin/analytics/usage` - API usage statistics
- `GET /admin/analytics/trending` - Trending topics
- `GET /admin/reports/generate?type={type}` - Generate reports

---

## 6. Key Features Highlight

### For Users
1. **Easy Paper Discovery** - Search and find relevant research instantly
2. **AI-Powered Insights** - Get summaries without reading full papers
3. **Collaborative Research** - Work with teams on literature reviews
4. **Smart Recommendations** - Discover papers you didn't know about
5. **Organized Collections** - Tag, categorize, and annotate papers

### For Admins
1. **Complete Visibility** - Monitor all user and system activity
2. **Usage Analytics** - Understand how the system is being used
3. **Trend Analysis** - See what research areas are popular
4. **Performance Monitoring** - Track API health and response times
5. **Offline Access** - View data without internet connection

---

## 7. Important Implementation Notes

### Understanding & Explanation Requirements

Students must be able to clearly explain:

1. **System Architecture** - How Blazor, API, Database, and WinForms interact
2. **Data Flow** - How data moves from APIs → backend → database → frontend
3. **Component Interaction** - How different parts communicate
4. **Design Decisions** - Why you chose this tech stack and architecture
5. **API Integration** - How external APIs (SerpAPI, OpenAI, OpenAlex) are integrated
6. **Database Design** - Why the schema is structured this way

These explanations are critical for viva evaluation.

---

## 8. Development Phases (Suggested)

### Phase 1: Foundation (Week 1-2)
- Set up .NET Core Web API
- Create database schema in SQL Server
- Implement Google OAuth authentication
- Create basic API endpoints for Users and Groups

### Phase 2: Paper Discovery (Week 3-4)
- Integrate SerpAPI for paper search
- Create paper search endpoints
- Build Blazor UI for search
- Display search results

### Phase 3: AI Integration (Week 5-6)
- Integrate OpenAI API for summaries
- Integrate OpenAlex API for academic data
- Create summary display in Blazor
- Test API calls and caching

### Phase 4: Collaboration Features (Week 7-8)
- Implement group creation and management
- Add paper bookmarking and tagging
- Build group paper sharing
- Create group collaboration UI

### Phase 5: Admin Dashboard (Week 9-10)
- Build Windows Forms admin app
- Create API endpoints for analytics
- Implement user/group monitoring
- Add usage tracking

### Phase 6: Recommendations & Polish (Week 11-12)
- Implement recommendation engine
- Add trending analysis
- Polish UI/UX
- Complete documentation and viva preparation

---

## 9. Success Criteria

✅ **Technical Implementation**
- All three components (Blazor, API, WinForms) working together
- External APIs integrated successfully
- Database properly normalized and functional
- Authentication working via Google OAuth
- Admin dashboard accessing data via API

✅ **Functionality**
- Users can search papers, get summaries, create groups
- Teams can collaborate on research
- Admin can monitor activity and generate reports
- Data is organized and retrievable

✅ **Understanding & Communication**
- Student can explain architecture clearly
- Can describe data flow and interactions
- Can justify design decisions
- Demonstrates understanding of layered architecture
- Can explain API integration and local sync concept

✅ **Code Quality**
- Clean, readable code with comments
- Proper error handling
- Secure authentication and authorization
- Efficient database queries
- Following .NET best practices

---

## 10. Evaluation Criteria

| Area | Weight | Key Points |
|------|--------|-----------|
| **Implementation** | 40% | All 3 parts working, features complete, no major bugs |
| **Architecture & Design** | 25% | Proper layering, API design, database design, integration approach |
| **Understanding** | 20% | Ability to explain how system works, design decisions, data flow |
| **Documentation & Viva** | 15% | Clear documentation, can discuss and defend project in viva |

---

## 11. Resources & References

- **SerpAPI Docs:** https://serpapi.com/docs
- **OpenAI API Docs:** https://platform.openai.com/docs
- **ASP.NET Core:** https://learn.microsoft.com/en-us/aspnet/core
- **Blazor:** https://learn.microsoft.com/en-us/aspnet/core/blazor
- **Bootstrap:** https://getbootstrap.com/
- **Tailwind:** https://tailwindcss.com/

---

## 12. Next Steps

1. **Setup Development Environment** - Install .NET SDK, Visual Studio, SQL Server
2. **Create Project Structure** - Set up Blazor, API, and WinForms projects
3. **Get API Keys** - Register for SerpAPI, OpenAI, Google OAuth
4. **Design Database** - Create ERD and tables
5. **Start Building** - Follow the development phases above