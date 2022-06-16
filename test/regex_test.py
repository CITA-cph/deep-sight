import re

def principal_period(s):
    i = (s+s).find(s, 1, -1)
    return None if i == -1 else s[:i]

def solution(s):
    # Your code here
    pattern = principal_period(s)
    return len(re.findall(pattern, s))


print(solution("abcabcabcabc"))
print(solution("abccbaabccba"))